using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.UserGroups.Models;
using MonitorQA.Data;
using UserEntity = MonitorQA.Data.Entities.User;

namespace MonitorQA.Api.Modules.UserGroups
{
    [Route("[controller]")]
    public class UserGroupsController : AuthorizedController
    {
        private readonly SiteContext _context;

        public UserGroupsController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet("detailed")]
        [ProducesResponseType(typeof(UserGroupsDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetailed()
        {
            if (!CurrentUser.Role.CanManageUsers)
            {
                return Forbid();
            }

            var totalUsersCount = await _context.Users
                .Where(UserEntity.Predicates.SameCompany(CurrentUser))
                .Where(UserEntity.Predicates.NotDeleted())
                .CountAsync();

            var ungroupedGroupUsersCount = await _context.Users
                .Where(UserEntity.Predicates.SameCompany(CurrentUser))
                .Where(UserEntity.Predicates.NotDeleted())
                .Where(u => !u.UserUserGroups.Any())
                .CountAsync();

            var groupsData = await _context.UserGroups
                .AsNoTracking()
                .Where(g => g.CompanyId == CurrentUser.CompanyId)
                .Select(g => new UserGroupModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    UserIds = g.UserUserGroups
                        .Where(uug => !uug.User.IsDeleted)
                        .Select(uug => uug.UserId),
                }).ToListAsync();

            var result = new UserGroupsDetailsModel
            {
                AvailableUsersCount = totalUsersCount,
                UngroupedUsersCount = ungroupedGroupUsersCount,
                Groups = groupsData
            };


            return Ok(result);
        }

        [HttpGet("concise")]
        [ProducesResponseType(typeof(IEnumerable<IdNamePairModel<Guid>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConcise()
        {
            if (!CurrentUser.Role.CanManageUsers)
            {
                return Forbid();
            }

            return Ok(await _context.UserGroups
                .AsNoTracking()
                .Where(g => g.CompanyId == CurrentUser.CompanyId)
                .Select(g => new IdNamePairModel<Guid>
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserGroupModel model)
        {
            if (!CurrentUser.Role.CanManageUsers)
                return Forbid();

            var userGroup = model.CreateEntity(CurrentUser.CompanyId);
            _context.UserGroups.Add(userGroup);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUserGroupModel model)
        {
            if (!CurrentUser.Role.CanManageUsers)
                return Forbid();

            await model.UpdateEntity(_context, CurrentUser.CompanyId);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("assign-user-group")]
        public async Task<IActionResult> AssignUserGroupToAuditObject([FromBody] UserGroupToAuditObjectModel model)
        {
            if (!CurrentUser.Role.CanManageUsers)
                return Forbid();

            var companyId = CurrentUser.CompanyId;
            await model.AssignAndSave(_context, companyId);

            return Ok();
        }

        [HttpPost]
        [Route("remove-user-group")]
        public async Task<IActionResult> RemoveUserGroupFromAuditObject([FromBody] UserGroupToAuditObjectModel model)
        {
            if (!CurrentUser.Role.CanManageUsers)
                return Forbid();

            await model.RemoveAndSave(_context, CurrentUser.CompanyId);

            return Ok();
        }

        [HttpPost]
        [Route("assign-user")]
        public async Task<IActionResult> AssignUserAuditObject([FromBody] UserToAuditObjectModel model)
        {
            if (!CurrentUser.Role.CanManageUsers)
                return Forbid();

            await model.AssignAndSave(_context, CurrentUser);

            return Ok();
        }

        [HttpPost]
        [Route("remove-user")]
        public async Task<IActionResult> RemoveUserFromAuditObject([FromBody] UserToAuditObjectModel model)
        {
            if (!CurrentUser.Role.CanManageUsers)
                return Forbid();

            await model.RemoveAndSave(_context, CurrentUser.CompanyId);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel<Guid> model)
        {
            if (!CurrentUser.Role.CanManageUsers)
                return Forbid();

            var companyId = CurrentUser.CompanyId;
            var userGroups = await _context.UserGroups
                .Include(ug => ug.UserUserGroups)
                .Include(ug => ug.AuditObjectUsers)
                .Include(ug => ug.AuditObjectUserGroups)
                .Where(ug => model.Ids.Contains(ug.Id) && ug.CompanyId == companyId)
                .ToListAsync();

            var userUserGroups = userGroups.SelectMany(ug => ug.UserUserGroups);
            var auditObjectUsers = userGroups.SelectMany(ug => ug.AuditObjectUsers);
            var auditObjectUserGroups = userGroups.SelectMany(ug => ug.AuditObjectUserGroups);

            _context.UserUserGroups.RemoveRange(userUserGroups);
            _context.AuditObjectUserGroups.RemoveRange(auditObjectUserGroups);
            _context.AuditObjectUsers.RemoveRange(auditObjectUsers);
            _context.UserGroups.RemoveRange(userGroups);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
