using Microsoft.EntityFrameworkCore;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.UserGroups.Models
{
    public class UserToAuditObjectModel
    {
        public Guid AuditObjectId { get; set; }

        public Guid UserId { get; set; }

        public async Task AssignAndSave(SiteContext context, User currentUser)
        {
            var companyId = currentUser.CompanyId;
            var user = await context.Users
                .Include(u => u.AuditObjectUsers)
                .SingleAsync(u => u.Id == UserId && u.CompanyId == companyId);

            var auditObject = await context.AuditObjects
                .Where(AuditObject.Predicates.UserHasAccess(currentUser))
                .Include(ug => ug.AuditObjectUsers)
                .SingleAsync(ao => ao.Id == AuditObjectId);

            var auditObjectUser = new AuditObjectUser
            {
                AuditObject = auditObject,
                User = user,
            };

            context.AuditObjectUsers.Add(auditObjectUser);

            await context.SaveChangesAsync();
        }

        public async Task RemoveAndSave(SiteContext context, Guid companyId)
        {
            var auditObject = await context.AuditObjectUsers
                .SingleAsync(aou => aou.AuditObjectId == AuditObjectId
                                && aou.UserId == UserId
                                && aou.AuditObject.CompanyId == companyId);

            context.AuditObjectUsers.Remove(auditObject);

            await context.SaveChangesAsync();
        }

    }
}
