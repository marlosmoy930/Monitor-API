using System;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.AuditList
{
    public abstract class AuditListRequestModel : ListRequestModel
    {
        public Guid? TemplateId { get; set; }

        public Guid? AuditObjectId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public AssignmentType? Assignment { get; set; }

        public DueDateStatusEnum? DueDateStatus { get; set; }
        
        protected IQueryable<Audit> ApplyFiltersBase(IQueryable<Audit> query, User user)
        {
            if (AuditObjectId.HasValue)
                query = query.Where(audit => audit.AuditObjectId == AuditObjectId);

            if (TemplateId.HasValue)
                query = query.Where(audit => audit.AuditSchedule.TemplateId == TemplateId);

            if (Assignment == AssignmentType.My)
                query = query.Where(audit => audit.Assignees.Any(u => u.UserId == user.Id));

            if (Assignment == AssignmentType.Unassigned)
                query = query.Where(audit => !audit.Assignees.Any());

            if (Search != null)
                query = query.Where(audit => audit.Number.Contains(Search) ||
                                             audit.AuditSchedule.Name.Contains(Search) ||
                                             audit.AuditObject.Name.Contains(Search));

            if (DueDateStatus == DueDateStatusEnum.DueSoon)
                query = query.Where(Audit.Predicates.AuditStatuses.IsDueSoon);

            if (DueDateStatus == DueDateStatusEnum.PastDue)
                query = query.Where(Audit.Predicates.AuditStatuses.IsDue);

            return query;
        }

        protected abstract Expression<Func<Audit, object>> GetOrderByExpression(string orderBy);

        public IQueryable<Audit> ApplyOrdering(IQueryable<Audit> query)
        {
            query = OrderByDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(GetOrderByExpression(OrderBy))
                : query.OrderBy(GetOrderByExpression(OrderBy));

            return query;
        }
    }
}