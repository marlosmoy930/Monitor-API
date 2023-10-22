using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.AuditObjects.Models;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Domain;

namespace MonitorQA.Api.Modules.AuditObjects
{
    [Route("audit/objects")]
    [ApiController]
    public class AuditObjectsController : AuthorizedController
    {
        private readonly SiteContext _context;

        public AuditObjectsController(SiteContext context)
            : base(context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("concise")]
        public async Task<IActionResult> GetConciseList()
        {
            var list = await _context.AuditObjects
                .AsNoTracking()
                .Where(AuditObject.Predicates.BelongsToCompany(CurrentUser.CompanyId))
                .Where(ao => !ao.IsDeleted)
                .Select(ao => new
                {
                    ao.Id,
                    ao.Name
                })
                .OrderBy(ao => ao.Name)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet]
        [Route("detailed")]
        [ProducesResponseType(typeof(ListPageResult<AuditObjectItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList([FromQuery] AuditObjectListRequest model)
        {
            var query = model.Filter(_context.AuditObjects.AsNoTracking(), CurrentUser.CompanyId);

            var count = await query.CountAsync();

            query = model.ApplyOrder(query);

            var auditObjects = await query
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .Select(AuditObjectItem.GetSelectExpression())
                .ToListAsync();

            var result = ListPageResult<AuditObjectItem>.Create(auditObjects, count, model);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(AuditObjectItem), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid id)
        {
            var auditObjectItem = await _context.AuditObjects
                .AsNoTracking()
                .Where(AuditObject.Predicates.UserHasAccess(CurrentUser))
                .Where(ao => ao.Id == id)
                .Select(AuditObjectItem.GetSelectExpression())
                .SingleAsync();

            return Ok(auditObjectItem);
        }

        [HttpPost]
        [ProducesResponseType(typeof(IdModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(AuditObjectModel model)
        {
            var participants = await model.CreateParticipants(_context);

            var auditObjectUserGroups = model.ParticipantUserGroupIds?
                .Select(userGroupId => new AuditObjectUserGroup
                {
                    UserGroupId = userGroupId,
                })
                .ToList() ?? new List<AuditObjectUserGroup>();

            var auditObject = new AuditObject
            {
                Id = Guid.NewGuid(),
                CompanyId = CurrentUser.CompanyId,
                Name = model.Name,
                Longitude = model.GeoAddress?.Lng,
                Latitude = model.GeoAddress?.Lat,
                AddressName = model.GeoAddress?.Name,
                Address = model.GeoAddress?.Address,
                CreatedAt = DateTime.UtcNow,
                AuditObjectTags = model.TagsIds?.Select(tid => new AuditObjectTag
                {
                    TagId = tid
                }).ToList(),
                AuditObjectUsers = participants,
                AuditObjectAuditObjectGroups = model.CreateAuditObjectAuditObjectGroups(),
                AuditObjectUserGroups = auditObjectUserGroups,
            };

            if (model.AuditObjectGroupIds != null && model.AuditObjectGroupIds.Any())
            {
                var domainModel = new AuditObjectDomainModel(auditObject.Id, new List<Guid>());
                await domainModel.AddAndRemovePendingAudits(_context, model.AuditObjectGroupIds, CurrentUser);
            }

            _context.AuditObjects.Add(auditObject);
            await _context.SaveChangesAsync();

            return Ok(new IdModel { Id = auditObject.Id });
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(Guid id, AuditObjectModel model)
        {
            var isSuccess = await model.UpdateEntity(_context, id, CurrentUser);

            if (!isSuccess)
                return BadRequest();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel model)
        {
            var hasSchedules = await _context.AuditSchedules
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .Where(s => s.AuditScheduleAuditObjects.Any(ao => model.Ids.Contains(ao.AuditObjectId)))
                .AnyAsync();

            if (hasSchedules)
                return Conflict("audit-object/has-attached-schedules");

            var list = await _context.AuditObjects
                .AsQueryable()
                .Where(AuditObject.Predicates.UserHasAccess(CurrentUser))
                .Where(ao => model.Ids.Contains(ao.Id))
                .ToListAsync();

            foreach (var item in list)
                item.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}