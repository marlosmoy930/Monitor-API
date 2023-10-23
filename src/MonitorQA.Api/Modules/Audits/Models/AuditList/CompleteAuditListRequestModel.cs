using System;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.AuditList
{
    public class CompleteAuditListRequestModel : AuditListRequestModel
    {
        public decimal? ScoreMin { get; set; }

        public decimal? ScoreMax { get; set; }

        public IQueryable<Audit> ApplyFilters(IQueryable<Audit> query, User user)
        {
            query = ApplyFiltersBase(query, user);

            if (StartDate.HasValue && EndDate.HasValue)
            {
                var from = StartDate.Value.Date;
                var to = EndDate.Value.Date.AddDays(1);
                query = query.Where(audit => audit.CompleteDate >= from && audit.CompleteDate < to);
            }

            if (ScoreMin.HasValue && ScoreMin != 0)
                query = query.Where(audit => audit.Score >= ScoreMin);

            if (ScoreMax.HasValue)
                query = query.Where(audit => audit.Score <= ScoreMax);

            return query;
        }

        protected override Expression<Func<Audit, object>> GetOrderByExpression(string orderBy)
        {
            switch (orderBy)
            {
                case "name": return audit => audit.Number;
                case "completeDate": return audit => audit.CompleteDate;
                case "startDate": return audit => audit.StartDate;
                case "endDate": return audit => audit.EndDate;
                default: return audit => audit.AuditSchedule.Name;
            }
        }
    }
}