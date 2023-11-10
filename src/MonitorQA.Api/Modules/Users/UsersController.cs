using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.Users.Models;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Firebase;
using MonitorQA.I18n;
using MonitorQA.Notifications;

namespace MonitorQA.Api.Modules.Users
{
    [Route("[controller]")]
    public class UsersController : AuthorizedController
    {
        private readonly CloudMessagePublisher _publisher;
        private readonly SiteContext _context;
        private readonly FirebaseUsersService _firebaseUsersService;
        private readonly Random _rnd = new Random();

        public UsersController(
            CloudMessagePublisher publisher,
            SiteContext context,
            FirebaseUsersService firebaseUsersService)
            : base(context)
        {
            this._publisher = publisher;
            _context = context;
            _firebaseUsersService = firebaseUsersService;
        }


        [HttpGet("myuser")]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserId();
            if (!Guid.TryParse(userId, out var userIdAsGuid))
            {
                await _firebaseUsersService.DeleteUsers(new[] { userId });
                return Conflict("auth/user-not-found");
            }

            var result = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id.ToString() == userId)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Phone,
                    u.Locale,
                    u.HasResetPassword,
                    Company = new
                    {
                        u.Company.Id,
                        u.Company.Name
                    },
                    Role = new
                    {
                        u.Role.Id,
                        u.Role.Name,

                        u.Role.CanDoAudits,
                        u.Role.CanAssignAudits,
                        u.Role.CanScheduleAudits,
                        u.Role.CanManageAudits,
                        u.Role.CanViewAuditsResults,

                        // TOOD Remove. It is Obsolete and used fpr backward compatibility with Mobile
                        HasAccessToUpcomingAudits = u.Role.CanDoAudits,

                        u.Role.CanDoCorrectiveActions,
                        u.Role.CanApproveCorrectiveActions,
                        u.Role.CanAssignCorrectiveActions,

                        u.Role.CanManageAuditObjects,
                        u.Role.CanManageTemplates,
                        u.Role.CanManageUsers,
                        u.Role.CanManageTags,
                        u.Role.CanManageRoles,
                        u.Role.CanManageScoreSystems,
                    }
                })
                .SingleAsync();

            return Ok(result);
        }

        [HttpGet]
        [Route("concise")]
        public async Task<IActionResult> GetByRole()
        {
            var list = await _context.Users.AsQueryable()
                .Where(u => u.CompanyId == CurrentUser.CompanyId && !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.Name
                })
                .OrderBy(u => u.Name)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet]
        [Route("detailed")]
        [ProducesResponseType(typeof(ListPageResult<UserDetail>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersDetailed([FromQuery] UsersRequestModel model)
        {
            var query = model.Filter(_context.Users.AsNoTracking(), CurrentUser.CompanyId);

            var count = await query.CountAsync();

            query = model.ApplyOrder(query);

            var users = await query
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .Select(UserDetail.GetSelectExpression())
                .ToListAsync();

            var result = ListPageResult<UserDetail>.Create(users, count, model);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUsers([FromBody] IdsArrayModel model)
        {
            var companyUsers = await _context.Users
                .AsQueryable()
                .Where(u => u.CompanyId == CurrentUser.CompanyId)
                .Where(u => model.Ids.Contains(u.Id))
                .ToListAsync();

            await _firebaseUsersService.DeleteUsers(companyUsers.Select(u => u.Id));

            foreach (var user in companyUsers)
            {
                user.RoleId = null;
                user.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("suspend")]
        public async Task<IActionResult> SuspendUsers([FromBody] IdsArrayModel model)
        {
            await ChangeIsActive(model.Ids, false);
            return Ok();
        }

        [HttpPost("activate")]
        public async Task<IActionResult> ActivateUsers([FromBody] IdsArrayModel model)
        {
            await ChangeIsActive(model.Ids, true);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel model)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Locale = Locales.Default,
            };
            model.UpdateUserEntity(user, CurrentUser.CompanyId);

            var role = await _context.Roles
                .AsNoTracking()
                .SingleAsync(r => r.Id == model.RoleId && r.CompanyId == CurrentUser.CompanyId);

            string password = _rnd.Next(10000000, 99999999).ToString();

            try
            {
                await _firebaseUsersService.CreateUser(CurrentUser.CompanyId, user, role, password);
            }
            catch (FirebaseException e)
            {
                return Conflict(e.Error);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new UserInviteMessage
            {
                InvitingUserId = CurrentUser.Id,
                NewUserId = user.Id,
                TempPassword = password
            };
            await _publisher.Publish(request);

            return Ok(new IdModel { Id = user.Id });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserModel model)
        {
            var user = await _context
                .Users
                .Include(u => u.UserTags)
                .Include(u => u.AuditObjectUsers)
                .Include(u => u.UserUserGroups)
                .Include(u => u.Role)
                .Where(u => u.Id == userId)
                .Where(u => u.CompanyId == CurrentUser.CompanyId)
                .FirstOrDefaultAsync();

            if (user == null)
                return Conflict();

            if (user.Role == null)
                return Conflict();

            model.UpdateUserEntity(user, CurrentUser.CompanyId);

            await model.UpdateAuditObjectUsers(_context, user, CurrentUser.CompanyId);

            if (!string.IsNullOrEmpty(model.Password))
                user.HasResetPassword = true;

            var userRole = await _context.Roles.SingleAsync(r => r.Id == user.RoleId);

            await _firebaseUsersService.UpdateUser(CurrentUser.CompanyId, user, userRole, model.Password);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task ChangeIsActive(IEnumerable<Guid> ids, bool isActive)
        {
            var companyUsers = await _context.Users.AsQueryable()
                .Include(u => u.Role)
                .Where(u => u.CompanyId == CurrentUser.CompanyId && ids.Any(id => id == u.Id))
                .ToListAsync();

            companyUsers.ForEach(cur => cur.IsActive = isActive);
            await _context.SaveChangesAsync();

            foreach (var user in companyUsers)
            {
                await _firebaseUsersService.UpdateUser(CurrentUser.CompanyId, user, user.Role, disabled: !isActive);
            }
        }
    }
}
