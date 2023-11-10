using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Users.Models
{
    public class UserModel
    {
        public string? Email { get; set; }

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public bool? IsActive { get; set; } = true;

        public Guid? RoleId { get; set; }

        public List<Guid>? TagsIds { get; set; }

        public string? Locale { get; set; }

        public string? Password { get; set; }

        public IEnumerable<Guid>? UserGroupIds { get; set; }

        public void UpdateUserEntity(User user, Guid companyId)
        {
            user.CompanyId = companyId;

            if (Email != null)
                user.Email = Email;

            if (FullName != null)
                user.Name = FullName;

            if (Phone != null)
                user.Phone = Phone;

            if (IsActive.HasValue)
                user.IsActive = IsActive.Value;

            if (RoleId.HasValue)
                user.RoleId = RoleId;

            if (TagsIds != null)
                user.UserTags = TagsIds.Select(id => new UserTag { TagId = id }).ToList();

            if (!string.IsNullOrEmpty(Locale))
                user.Locale = Locale.ToUpper(CultureInfo.InvariantCulture);

            if (UserGroupIds != null)
            {
                user.UserUserGroups = UserGroupIds
                    .Select(userGroupId => new UserUserGroup
                    {
                        UserGroupId = userGroupId,
                        UserId = user.Id,
                    })
                    .ToList();
            }
        }

        public async Task UpdateAuditObjectUsers(SiteContext context, User user, Guid companyId)
        {
            if (UserGroupIds == null) return;

            var auditObjectUsers = new List<AuditObjectUser>();
            
            var ungroupedAuditObjects = user.AuditObjectUsers
                .Where(aou => !aou.UserGroupId.HasValue)
                .Select(aou => aou.AuditObjectId)
                .Distinct()
                .Select(auditObjectId => new AuditObjectUser
                {
                    AuditObjectId = auditObjectId,
                    UserId = user.Id,
                })
                .ToList();
            auditObjectUsers.AddRange(ungroupedAuditObjects);

            if (UserGroupIds.Any())
            {
                var auditObjectsByUserGroups = await context.AuditObjectUsers
                                        .AsNoTracking()
                                        .Where(aou => aou.UserGroupId.HasValue)
                                        .Where(aou => aou.AuditObject.CompanyId == companyId)
                                        .Where(aou => UserGroupIds.Contains(aou.UserGroupId.Value))
                                        .Select(aou => new { aou.AuditObjectId, aou.UserGroupId })
                                        .Distinct()
                                        .ToListAsync();

                var groupedAuditUserObjects = auditObjectsByUserGroups
                    .Select(aou => new AuditObjectUser
                    {
                        AuditObjectId = aou.AuditObjectId,
                        UserGroupId = aou.UserGroupId,
                        UserId = user.Id,
                    });

                auditObjectUsers.AddRange(groupedAuditUserObjects);
            }

            context.AuditObjectUsers.RemoveRange(user.AuditObjectUsers);
            context.AuditObjectUsers.AddRange(auditObjectUsers);
        }
    }
}
