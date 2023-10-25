using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.AuditSchedules.Models;
using MonitorQA.Api.Modules.AuditSchedules.Models.AdHoc;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Domain;

namespace MonitorQA.Api.Modules.AuditSchedules
{
    [Route("audit/schedules")]
    [ApiController]
    public class AuditSchedulesController : AuthorizedController
    {
        private readonly SiteContext _context;

        public AuditSchedulesController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] ListRequestModel model)
        {
            if (!CurrentUser.Role.CanScheduleAudits)
                return Forbid();

            var query = _context.AuditSchedules
                .AsNoTracking()
                .Where(AuditSchedule.Predicates.UserHasAccess(CurrentUser))
                .Where(s => !s.IsDeleted);

            if (!string.IsNullOrEmpty(model.Search))
                query = query.Where(s => s.Template.Name.Contains(model.Search)
                                         || s.Name.Contains(model.Search));

            var ctn = await query.CountAsync();

            var query2 = query
                .Select(s => new AuditScheduleResponseModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Template = new EntityConciseModel
                    {
                        Id = s.TemplateId,
                        Name = s.Template.Name
                    },
                    CurrentAudit = s.Audits.Where(a => !a.IsCompleted)
                        .Select(a => new AuditDateRangeModel
                        {
                            StartDate = a.StartDate,
                            EndDate = a.EndDate
                        })
                        .FirstOrDefault(),
                    Repeat = new RepeatSettingsModel
                    {
                        RepeatPattern = s.RepeatPattern,
                        RepeatEvery = s.RepeatEvery,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        EndAfterTimes = s.RepeatEndAfterTimes
                    },
                    AuditObjects = s.AuditScheduleAuditObjects.Select(r => new EntityConciseModel
                    {
                        Id = r.AuditObjectId,
                        Name = r.AuditObject.Name
                    }),
                    AuditObjectGroups = s.AuditScheduleAuditObjectGroups.Select(r => new EntityConciseModel
                    {
                        Id = r.AuditObjectGroupId,
                        Name = r.AuditObjectGroup.Name
                    }),
                    Tags = s.AuditScheduleTags
                        .OrderBy(tags => tags.Tag.Name)
                        .Select(tags => new TagConciseModel
                        {
                            Id = tags.Tag.Id,
                            Name = tags.Tag.Name,
                            Color = tags.Tag.Color
                        })
                });

            Expression<Func<AuditScheduleResponseModel, object>> GetOrderByExpression(string orderBy)
            {
                switch (orderBy)
                {
                    case "name": return s => s.Name;
                    case "auditDueDate": return s => s.CurrentAudit.EndDate;
                    default: return s => s.Name;
                }
            }

            query2 = model.OrderByDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? query2.OrderByDescending(GetOrderByExpression(model.OrderBy))
                : query2.OrderBy(GetOrderByExpression(model.OrderBy));

            var data = await query2.Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .ToListAsync();


            return Ok(new ListPageResult(data, ctn, model));
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (!CurrentUser.Role.CanScheduleAudits)
                return Forbid();

            var result = await _context.AuditSchedules
                .AsNoTracking()
                .Where(AuditSchedule.Predicates.UserHasAccess(CurrentUser))
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    AuditObjects = s.AuditScheduleAuditObjects.Select(r => new EntityConciseModel
                    {
                        Id = r.AuditObjectId,
                        Name = r.AuditObject.Name
                    }),
                    AuditObjectGroups = s.AuditScheduleAuditObjectGroups.Select(r => new EntityConciseModel
                    {
                        Id = r.AuditObjectGroupId,
                        Name = r.AuditObjectGroup.Name
                    }),
                    Template = new { Id = s.TemplateId, Name = s.Template.Name },
                    s.CreatedAt,
                    Repeat = new
                    {
                        s.RepeatPattern,
                        s.RepeatEvery,
                        s.StartDate,
                        s.EndDate,
                        EndAfterTimes = s.RepeatEndAfterTimes
                    },
                }).FirstOrDefaultAsync();

            if (result == null)
                return Forbid();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditScheduleRequestModel model)
        {
            if (!CurrentUser.Role.CanScheduleAudits)
                return Forbid();

            var template = await _context.Templates
                .AsNoTracking()
                .FirstOrDefaultAsync(ao => ao.CompanyId == CurrentUser.CompanyId && ao.Id == model.TemplateId);

            if (template == null)
                return Forbid();

            if (string.IsNullOrEmpty(model.Name))
                model.Name = template.Name;

            var schedule = new AuditSchedule
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            model.UpdateScheduleEntity(schedule);

            var domainModel = new ScheduleDomainModel(schedule.StartDate.Value, schedule);
            await domainModel.LoadNewAuditObjectIds(_context, CurrentUser, model.AuditObjectIds, model.AuditObjectGroupIds);
            schedule.Audits = domainModel.CreatePendingAudits();

            _context.AuditSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return Ok(new IdModel { Id = schedule.Id });
        }

        [HttpPost]
        [Route("ad-hoc")]
        public async Task<IActionResult> CreateAdHocAudit(AdHocAuditModel model)
        {
            if(!CurrentUser.Role.CanDoAudits)
                return Forbid();

            var template = await _context.Templates
                .AsNoTracking()
                .FirstOrDefaultAsync(ao => ao.CompanyId == CurrentUser.CompanyId && ao.Id == model.TemplateId);

            if (template == null)
                return Forbid();

            if (string.IsNullOrEmpty(model.Name))
                model.Name = template.Name;

            var schedule = new AuditSchedule
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            model.UpdateScheduleEntity(schedule);

            var domainModel = new ScheduleDomainModel(schedule.StartDate.Value, schedule);
            await domainModel.LoadNewAuditObjectIds(_context, CurrentUser, new[] { model.AuditObjectId }, new List<Guid>());
            schedule.Audits = domainModel.CreatePendingAudits();

            var audit = schedule.Audits.Single();
            audit.Assignees = new List<AuditAssignee>
            {
                new AuditAssignee
                {
                    AuditId = audit.Id,
                    UserId = CurrentUser.Id
                }
            };

            _context.AuditSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return Ok(new IdModel { Id = schedule.Id });
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AuditScheduleRequestModel model)
        {
            if (!CurrentUser.Role.CanScheduleAudits)
                return Forbid();

            var template = await _context.Templates
                .AsNoTracking()
                .Where(Template.Predicates.BelongsToUserCompany(CurrentUser))
                .Where(t => t.Id == model.TemplateId)
                .SingleOrDefaultAsync();

            if (template == null)
                return Forbid();


            if (string.IsNullOrEmpty(model.Name))
                model.Name = template.Name;

            var schedule = await _context.AuditSchedules
                .AsQueryable()
                .Where(AuditSchedule.Predicates.UserHasAccess(CurrentUser))
                .Include(s => s.AuditScheduleTags)
                .Include(s => s.AuditScheduleAuditObjects)
                .Include(s => s.AuditScheduleAuditObjectGroups)
                .SingleAsync(s => s.Id == id);

            model.UpdateScheduleEntity(schedule);

            var domainModel = new ScheduleDomainModel(DateTime.UtcNow, schedule);
            await domainModel.RemoveUpcomingAudits(_context, CurrentUser);
            await domainModel.LoadNewAuditObjectIds(_context, CurrentUser, model.AuditObjectIds, model.AuditObjectGroupIds);
            schedule.Audits = domainModel.CreatePendingAudits();

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel model)
        {
            if (!CurrentUser.Role.CanScheduleAudits)
                return Forbid();

            var entities = await _context.AuditSchedules
                .AsQueryable()
                .Where(AuditSchedule.Predicates.UserHasAccess(CurrentUser))
                .Where(t => model.Ids.Any(id => id == t.Id))
                .ToListAsync();

            var ids = entities.Select(s => s.Id).ToList();

            // todo reuse predicates with ScheduleDomainModel.RemoveUpcomingAudits
            var auditsToDelete = await _context.Audits
                .AsQueryable()
                .Where(a => a.AuditScheduleId.HasValue && ids.Contains(a.AuditScheduleId.Value))
                .Where(a => !a.IsCompleted && !a.IsStarted)
                .Include(a => a.Notifications)
                .Include(a => a.Assignees)
                .ToListAsync();

            _context.RemoveRange(auditsToDelete);

            foreach (var auditSchedule in entities)
            {
                auditSchedule.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
