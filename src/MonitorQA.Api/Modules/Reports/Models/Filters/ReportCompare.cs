using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorQA.Api.Modules.Reports.Models.Filters
{
    public class ReportCompare
    {
        public List<Guid>? UserIds { get; set; }

        public List<Guid>? UserGroupIds { get; set; }

        public List<Guid>? AuditObjectIds { get; set; }

        public List<Guid>? AuditObjectGroupIds { get; set; }

        public List<CompareItem<T>> GetCompareItems<T>(List<T> items)
            where T : class, ICloneable
        {
            var compareItems = new List<CompareItem<T>>();
            if (UserIds != null && UserIds.Any())
            {
                var userItems = UserIds
                    .Select(userId => CreateCompareItem(items, userId, CompareType.User));
                compareItems.AddRange(userItems);
            }

            if (UserGroupIds != null && UserGroupIds.Any())
            {
                var userGroupItems = UserGroupIds
                    .Select(userGroupId => CreateCompareItem(items, userGroupId, CompareType.UserGorup));
                compareItems.AddRange(userGroupItems);
            }

            if (AuditObjectIds != null && AuditObjectIds.Any())
            {
                var auditObjectIdItems = AuditObjectIds
                    .Select(auditObjectId => CreateCompareItem(items, auditObjectId, CompareType.AuditObject));
                compareItems.AddRange(auditObjectIdItems);
            }

            if (AuditObjectGroupIds != null && AuditObjectGroupIds.Any())
            {
                var auditObjectGroupItems = AuditObjectGroupIds
                    .Select(auditObjectGroupId => CreateCompareItem(items, auditObjectGroupId, CompareType.AuditObjectGroup));
                compareItems.AddRange(auditObjectGroupItems);
            }

            return compareItems;
        }

        public static CompareItem<T> CreateCompareItem<T>(
            List<T> items,
            Guid id,
            CompareType compareType) where T: class, ICloneable
        {
            var compare = new CompareItem<T>()
            {
                Id = id,
                CompareType = compareType,
                Items = items
                    .Select(s => s.Clone() as T)
                    .ToList()
            };

            return compare;
        }
    }
}
