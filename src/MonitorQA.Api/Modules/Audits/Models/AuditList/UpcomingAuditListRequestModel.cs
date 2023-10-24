using System;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.AuditList
{
    public class UpcomingAuditListRequestModel : AuditListRequestModel
    {
        public bool? InProgress { get; set; }

        public StartDateStatusEnum? StartDateStatus { get; set; }

        public IQueryable<Audit> ApplyFilters(IQueryable<Audit> query, User user, Role role)
        {
            query = ApplyFiltersBase(query, user);

            if (StartDate.HasValue && EndDate.HasValue)
            {
                var from = StartDate.Value.Date;
                var to = EndDate.Value.Date.AddDays(1);
                query = query.Where(audit => audit.EndDate >= from && audit.EndDate < to);
            }

            if (InProgress != null)
                query = query.Where(Audit.Predicates.AuditStatuses.IsInProgress);

            if (StartDateStatus == StartDateStatusEnum.Upcoming)
                query = query.Where(Audit.Predicates.AuditStatuses.IsInFuture);

            if (StartDateStatus == StartDateStatusEnum.Ready)
                query = query.Where(Audit.Predicates.AuditStatuses.IsReadyToStart);

            return query;
        }

        protected override Expression<Func<Audit, object>> GetOrderByExpression(string orderBy)
        {
            switch (orderBy)
            {
                case "name": return audit => audit.AuditSchedule.Name;
                case "completeDate": return audit => audit.CompleteDate;
                case "startDate": return audit => audit.StartDate;
                case "endDate": return audit => audit.EndDate;
                default: return audit => audit.AuditSchedule.Name;
            }
        }
    }
}