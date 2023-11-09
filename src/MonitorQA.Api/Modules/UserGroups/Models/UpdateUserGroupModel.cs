using Microsoft.EntityFrameworkCore;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.UserGroups.Models
{
    public class UpdateUserGroupModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Guid> UserIds { get; set; }

        public async Task UpdateEntity(SiteContext context, Guid companyId)
        {
            var userGroup = await context.UserGroups
                .Where(ug => ug.Id == Id && ug.CompanyId == companyId)
                .Include(ug => ug.UserUserGroups)
                .Include(ug => ug.AuditObjectUsers)
                .SingleAsync();

            userGroup.Name = Name;

            var auditObjectUsers = userGroup.AuditObjectUsers
                .Select(aou => aou.AuditObjectId)
                .Distinct()
                .SelectMany(auditObjectId => UserIds.Select(userId => new AuditObjectUser
                {
                    AuditObjectId = auditObjectId,
                    UserGroupId = Id,
                    UserId = userId,
                }))
                .ToList();

            context.AuditObjectUsers.RemoveRange(userGroup.AuditObjectUsers);
            context.AuditObjectUsers.AddRange(auditObjectUsers);

            var userUserGroups = UserIds
                .Select(userId => new UserUserGroup
                {
                    UserGroupId = Id,
                    UserId = userId,
                }).ToList();
            userGroup.UserUserGroups = userUserGroups;
        }
    }
}
