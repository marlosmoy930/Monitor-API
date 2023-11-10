using Microsoft.EntityFrameworkCore;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.UserGroups.Models
{
    public class UserGroupToAuditObjectModel
    {
        public Guid AuditObjectId { get; set; }

        public Guid UserGroupId { get; set; }

        public async Task AssignAndSave(SiteContext context, Guid companyId)
        {
            var userGroup = await context.UserGroups
                .Include(ug => ug.UserUserGroups)
                .SingleAsync(ug => ug.Id == UserGroupId && ug.CompanyId == companyId);

            var userIds = await context.UserUserGroups
                .Where(uug => uug.UserGroupId == UserGroupId)
                .Select(uug => uug.UserId)
                .ToListAsync();

            var auditObjectUsers = userIds
                .Select(userId => new AuditObjectUser
                {
                    AuditObjectId = AuditObjectId,
                    UserGroupId = UserGroupId,
                    UserId = userId,
                });

            var auditObjectUserGroup = new AuditObjectUserGroup
            {
                AuditObjectId = AuditObjectId,
                UserGroupId = UserGroupId,
            };
            context.AuditObjectUserGroups.Add(auditObjectUserGroup);

            context.AuditObjectUsers.AddRange(auditObjectUsers);

            await context.SaveChangesAsync();
        }

        public async Task RemoveAndSave(SiteContext context, Guid companyId)
        {
            var auditObjectUsers = await context.AuditObjectUsers
                .Where(aou => aou.AuditObjectId == AuditObjectId
                    && aou.UserGroupId == UserGroupId
                    && aou.AuditObject.CompanyId == companyId)
                .ToListAsync();

            var auditObjectUserGroup = await context.AuditObjectUserGroups
                .SingleAsync(aoug => aoug.AuditObjectId == AuditObjectId && aoug.UserGroupId == UserGroupId);

            context.AuditObjectUserGroups.Remove(auditObjectUserGroup);
            context.AuditObjectUsers.RemoveRange(auditObjectUsers);

            await context.SaveChangesAsync();
        }
    }
}
