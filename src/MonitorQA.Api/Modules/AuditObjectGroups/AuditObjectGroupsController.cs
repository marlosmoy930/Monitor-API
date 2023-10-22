using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.AuditObjectGroups.Models;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Domain;

namespace MonitorQA.Api.Modules.AuditObjectGroups
{
    [Route("[controller]")]
    public class AuditObjectGroupsController : AuthorizedController
    {
        private readonly SiteContext _context;

        public AuditObjectGroupsController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet("detailed")]
        [ProducesResponseType(typeof(AuditObjectUserDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetailed()
        {
            if (!CurrentUser.Role.CanManageAuditObjects)
            {
                return Forbid();
            }

            var totalObjectsCount = await _context.AuditObjects
                .Where(AuditObject.Predicates.BelongsToCompany(CurrentUser.CompanyId))
                .Where(AuditObject.Predicates.NotDeleted())
                .CountAsync();

            var ungroupedAuditObjectsCount = await _context.AuditObjects
                .Where(AuditObject.Predicates.BelongsToCompany(CurrentUser.CompanyId))
                .Where(AuditObject.Predicates.NotDeleted())
                .Where(ao => !ao.AuditObjectAuditObjectGroups.Any())
                .CountAsync();

            var groupsData = await _context.AuditObjectGroups
                .AsNoTracking()
                .Where(g => g.CompanyId == CurrentUser.CompanyId)
                .Select(g => new AuditObjectGroupModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    AuditObjectIds = g.AuditObjectAuditObjectGroups
                        .Where(g => !g.AuditObject.IsDeleted)
                        .Select(x => x.AuditObjectId),
                }).ToListAsync();

            var result = new AuditObjectUserDetailsModel
            {
                AvailableAuditObjectsCount = totalObjectsCount,
                UngroupedAuditObjectsCount = ungroupedAuditObjectsCount,
                Groups = groupsData
            };


            return Ok(result);
        }

        [HttpGet("concise")]
        [ProducesResponseType(typeof(IEnumerable<IdNamePairModel<Guid>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConcise()
        {
            if (!CurrentUser.Role.CanManageAuditObjects)
            {
                return Forbid();
            }

            return Ok(await _context.AuditObjectGroups
                .AsNoTracking()
                .Where(g => g.CompanyId == CurrentUser.CompanyId)
                .Where(g => g.AuditObjectAuditObjectGroups.Any())
                .Select(g => new IdNamePairModel<Guid>
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAuditObjectGroupModel model)
        {
            if (!CurrentUser.Role.CanManageAuditObjects)
                return Forbid();

            var auditObjectGroup = model.CreateEntity(CurrentUser.CompanyId);
            _context.AuditObjectGroups.Add(auditObjectGroup);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAuditObjectGroupModel model)
        {
            if (!CurrentUser.Role.CanManageAuditObjects)
                return Forbid();

            var auditObjectGroup = await _context.AuditObjectGroups
                .Include(x => x.AuditObjectAuditObjectGroups).ThenInclude(r => r.AuditObject)
                .Include(g => g.AuditScheduleAuditObjectGroups).ThenInclude(r => r.AuditSchedule)
                .SingleAsync(x => x.Id == model.Id && x.CompanyId == CurrentUser.CompanyId);

            var domainModel = new AuditObjectGroupDomainModel(auditObjectGroup);
            await domainModel.AddAndRemovePendingAudits(_context, model.AuditObjectIds);

            model.UpdateEntity(auditObjectGroup);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel<Guid> model)
        {
            if (!CurrentUser.Role.CanManageAuditObjects)
                return Forbid();

            var auditObjectGroups = await _context.AuditObjectGroups
                .Include(x => x.AuditObjectAuditObjectGroups).ThenInclude(r => r.AuditObject)
                .Include(g => g.AuditScheduleAuditObjectGroups).ThenInclude(r => r.AuditSchedule)
                .Where(AuditObjectGroup.Predicates.BelongsToUserCompany(CurrentUser))
                .Where(x => model.Ids.Contains(x.Id))
                .ToListAsync();

            foreach (var auditObjectGroup in auditObjectGroups)
            {
                var domainModel = new AuditObjectGroupDomainModel(auditObjectGroup);
                await domainModel.AddAndRemovePendingAudits(_context, new List<Guid>());
            }

            _context.AuditObjectGroups.RemoveRange(auditObjectGroups);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
