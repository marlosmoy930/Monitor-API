using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Linq;

namespace MonitorQA.Api.Modules.AuditObjects.Models
{
    public class AuditObjectListRequest : ListRequestModel
    {
        public Guid? AuditObjectGroupId { get; set; }

        public AuditObjectGroupFilterType GroupFilterType { get; set; }

        public IQueryable<AuditObject> Filter(IQueryable<AuditObject> query, Guid companyId)
        {
            query = query
                .Where(AuditObject.Predicates.BelongsToCompany(companyId))
                .Where(ao => !ao.IsDeleted);

            if (!string.IsNullOrEmpty(Search))
                query = query.Where(u => u.Name.Contains(Search));

            if (GroupFilterType == AuditObjectGroupFilterType.All)
            {
            }
            else if (GroupFilterType == AuditObjectGroupFilterType.ByGroup)
            {
                query = query.Where(ao => ao.AuditObjectAuditObjectGroups.Any(aoaog => aoaog.AuditObjectGroupId == AuditObjectGroupId.Value));
            }
            else if (GroupFilterType == AuditObjectGroupFilterType.Ungrouped)
            {
                query = query.Where(ao => !ao.AuditObjectAuditObjectGroups.Any());
            }
            else
            {
                throw new NotImplementedException($"{nameof(GroupFilterType)} = {GroupFilterType}");
            }

            return query;
        }

        public IOrderedQueryable<AuditObject> ApplyOrder(IQueryable<AuditObject> query)
        {
            if (OrderBy == OrderByNameFieldName)
            {
                return IsAscendingOrderDirection
                    ? query.OrderBy(s => s.Name)
                    : query.OrderByDescending(s => s.Name);
            }

            throw new NotImplementedException($"{nameof(OrderBy)} is {OrderBy}");
        }
    }
}
