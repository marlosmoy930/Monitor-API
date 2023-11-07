using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Roles
{
    [Route("[controller]")]
    public class RolesController : AuthorizedController
    {
        private readonly SiteContext _context;

        public RolesController(SiteContext context)
            : base(context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("concise")]
        public async Task<IActionResult> GetConcise()
        {
            var res = await _context.Roles.AsQueryable()
                .Where(r => r.CompanyId == CurrentUser.CompanyId)
                .Select(r => new
                {
                    r.Id,
                    r.Name
                })
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(res);
        }

        [HttpGet]
        [Route("detailed")]
        public async Task<IActionResult> GetDetailed([FromQuery] ListRequestModel model)
        {
            var query = _context.Roles.AsQueryable()
                .Where(r => r.CompanyId == CurrentUser.CompanyId);

            var ctn = await query.CountAsync();

            if (!string.IsNullOrEmpty(model.Search))
                query = query.Where(u => u.Name.Contains(model.Search));

            Expression<Func<Role, string>> GetOrderByExpression(string orderBy)
            {
                switch (orderBy)
                {
                    case "name": return z => z.Name;
                    default: return z => z.Name;
                }
            }

            query = model.OrderByDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(GetOrderByExpression(model.OrderBy))
                : query.OrderBy(GetOrderByExpression(model.OrderBy));

            var data = await query
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsDefault,
                    UsersCount = r.Users.Count(u => !u.IsDeleted)
                }).ToListAsync();

            return Ok(new
            {
                Data = data,
                Meta = new PagingModel { PageNumber = model.PageNumber, PageSize = model.PageSize, TotalCount = ctn }
            });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetDetailed(Guid id)
        {
            var res = await _context.Roles.AsQueryable()
                .Where(r => r.CompanyId == CurrentUser.CompanyId && r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsDefault,

                    r.CanDoAudits,
                    r.CanAssignAudits,
                    r.CanScheduleAudits,
                    r.CanManageAudits,
                    r.CanViewAuditsResults,

                    r.CanDoCorrectiveActions,
                    r.CanApproveCorrectiveActions,
                    r.CanAssignCorrectiveActions,

                    r.CanManageAuditObjects,
                    r.CanManageTemplates,
                    r.CanManageUsers,
                    r.CanManageRoles,
                    r.CanManageTags,

                    r.CanManageScoreSystems,
                }).FirstOrDefaultAsync();

            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleModel model)
        {
            var role = new Role
            {
                CompanyId = CurrentUser.CompanyId,
                Name = model.Name,
                Description = model.Description ?? string.Empty,

                CanDoAudits = model.CanDoAudits ?? false,
                CanManageAudits = model.CanManageAudits ?? false,
                CanAssignAudits = model.CanAssignAudits ?? false,
                CanScheduleAudits = model.CanScheduleAudits ?? false,
                CanViewAuditsResults = model.CanViewAuditsResults ?? false,

                CanDoCorrectiveActions = model.CanDoCorrectiveActions ?? false,
                CanApproveCorrectiveActions = model.CanApproveCorrectiveActions ?? false,
                CanAssignCorrectiveActions = model.CanAssignCorrectiveActions ?? false,

                CanManageAuditObjects = model.CanManageAuditObjects ?? false,
                CanManageTemplates = model.CanManageTemplates ?? false,
                CanManageUsers = model.CanManageUsers ?? false,
                CanManageRoles = model.CanManageRoles ?? false,
                CanManageScoreSystems = model.CanManageScoreSystems ?? false,
            };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return Ok(new IdModel { Id = role.Id });
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoleModel model)
        {
            var role = await _context.Roles.AsQueryable()
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == CurrentUser.CompanyId);

            if (model.Name != null)
                role.Name = model.Name;

            if (model.Description != null)
                role.Description = model.Description;

            if (model.CanAssignAudits.HasValue)
                role.CanAssignAudits = model.CanAssignAudits.Value;

            if (model.CanDoAudits.HasValue)
                role.CanDoAudits = model.CanDoAudits.Value;

            if (model.CanManageAudits.HasValue)
                role.CanManageAudits = model.CanManageAudits.Value;

            if (model.CanScheduleAudits.HasValue)
                role.CanScheduleAudits = model.CanScheduleAudits.Value;

            if (model.CanViewAuditsResults.HasValue)
                role.CanViewAuditsResults = model.CanViewAuditsResults.Value;

            if (model.CanDoCorrectiveActions.HasValue)
                role.CanDoCorrectiveActions = model.CanDoCorrectiveActions.Value;

            if (model.CanApproveCorrectiveActions.HasValue)
                role.CanApproveCorrectiveActions = model.CanApproveCorrectiveActions.Value;

            if (model.CanAssignCorrectiveActions.HasValue)
                role.CanAssignCorrectiveActions = model.CanAssignCorrectiveActions.Value;

            if (model.CanManageAuditObjects.HasValue)
                role.CanManageAuditObjects = model.CanManageAuditObjects.Value;

            if (model.CanManageTemplates.HasValue)
                role.CanManageTemplates = model.CanManageTemplates.Value;

            if (model.CanManageUsers.HasValue)
                role.CanManageUsers = model.CanManageUsers.Value;

            if (model.CanManageRoles.HasValue)
                role.CanManageRoles = model.CanManageRoles.Value;

            if (model.CanManageTags.HasValue)
                role.CanManageTags = model.CanManageTags.Value;

            if (model.CanManageScoreSystems.HasValue)
                role.CanManageScoreSystems = model.CanManageScoreSystems.Value;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel model)
        {
            var roles = await _context.Roles
                .AsQueryable()
                .Where(r => r.CompanyId == CurrentUser.CompanyId)
                .Where(r => model.Ids.Contains(r.Id))
                .Include(r => r.Users)
                .ToListAsync();

            var roleIdsWithUsers = roles
                .Where(r => r.Users.Any())
                .Select(r => r.Id);

            if (roleIdsWithUsers.Any())
            {
                throw BusinessException.RoleExceptions.GetHasUsers(roleIdsWithUsers);
            }

            _context.RemoveRange(roles);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}