using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Linq;

namespace MonitorQA.Api.Modules.AuditTemplates.Models
{
    public class DetailedListRequest : ListRequestModel
    {
        public TemplateType? TemplateType { get; set; }

        public IQueryable<Template> Filter(IQueryable<Template> query, Guid companyId)
        {
            query = query
                .Where(t => t.CompanyId == companyId)
                .Where(t => !t.IsDeleted);

            if (!string.IsNullOrEmpty(Search))
                query = query.Where(t => t.Name.Contains(Search));

            if (TemplateType.HasValue)
                query = query.Where(t => t.TemplateType == TemplateType.Value);

            return query;
        }

        public IOrderedQueryable<Template> ApplyOrder(IQueryable<Template> query)
        {
            if (OrderBy.Equals(OrderByNameFieldName, StringComparison.OrdinalIgnoreCase))
            {
                return IsAscendingOrderDirection
                    ? query.OrderBy(t => t.Name)
                    : query.OrderByDescending(t => t.Name);
            }

            throw new NotImplementedException($"{nameof(OrderBy)} is {OrderBy}");
        }
    }
}