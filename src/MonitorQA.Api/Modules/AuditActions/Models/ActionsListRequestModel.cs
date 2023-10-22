using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditActions.Models
{
    public class ActionsListRequestModel : ListRequestModel
    {
        public const string OrderByDueDateFieldName = "dueDate";

        public const string OrderByApprovalDateFieldName = "completedAt";

        public new string OrderBy { get; set; } = string.Empty;

        public new string OrderByDirection { get; set; } = string.Empty;

        public Guid? TemplateId { get; set; }

        public Guid? AuditId { get; set; }

        public Guid? AuditItemId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public AssignmentType? Assignment { get; set; }

        public CorrectiveActionStatus? Status { get; set; }

        public IEnumerable<CorrectiveActionStatus>? Statuses { get; set; }

        public CorrectiveActionPriority? Priority { get; set; }

        public DueDateStatusEnum? DueDateStatus { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime? CompleteAuditDate { get; set; }

        public Guid? AuditObjectId { get; set; }

        public IQueryable<CorrectiveAction> ApplyFilter(IQueryable<CorrectiveAction> query, User currentUser)
        {
            if (Assignment == AssignmentType.My)
                query = query.Where(a => a.Assignees.Any(aa => aa.UserId == currentUser.Id));

            if (Assignment == AssignmentType.Unassigned)
                query = query.Where(a => !a.Assignees.Any());

            if (AuditId.HasValue)
                query = query.Where(a => a.AuditItem.AuditId == AuditId.Value);

            if (AuditItemId.HasValue)
                query = query.Where(a => a.AuditItemId == AuditItemId.Value);

            if (StartDate.HasValue && EndDate.HasValue)
                query = query.Where(a => a.CreatedAt < EndDate && a.DueDate > StartDate);

            if (Status != null)
                query = query.Where(a => a.Status == Status);

            if (Statuses != null)
                query = query.Where(a => Statuses.Contains(a.Status));

            if (Priority != null)
                query = query.Where(a => a.Priority == Priority);

            query = ApplyDueDateStatusFilter(query);

            if (Assignment == AssignmentType.My)
                query = query.Where(a => a.Assignees.Any(aa => aa.UserId == currentUser.Id));

            if (Assignment == AssignmentType.Unassigned)
                query = query.Where(a => !a.Assignees.Any());

            if (!string.IsNullOrEmpty(Search))
                query = query.Where(ca => ca.Name.Contains(Search));

            if (TemplateId.HasValue)
                query = query.Where(ca => ca.AuditItem.Audit.AuditSchedule.TemplateId == TemplateId);

            if (CreatedBy.HasValue)
                query = query.Where(ca => ca.CreatedById == CreatedBy.Value);

            if (CompleteAuditDate.HasValue)
                query = query.Where(a => a.AuditItem.Audit.CompleteDate.Value.Date == CompleteAuditDate.Value.Date);

            if (DueDate.HasValue)
                query = query.Where(ca => ca.DueDate.Value.Date == DueDate.Value.Date);

            if (ApprovalDate.HasValue)
                query = query.Where(ca => ca.ApprovedAt.Value.Date == ApprovalDate.Value.Date);

            if (AuditObjectId.HasValue)
                query = query.Where(ca => ca.AuditItem.Audit.AuditObjectId == AuditObjectId.Value);

            return query;
        }

        public IOrderedQueryable<CorrectiveAction> ApplyOrder(IQueryable<CorrectiveAction> query)
        {
            return OrderByDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(GetOrderByExpression(OrderBy))
                : query.OrderBy(GetOrderByExpression(OrderBy));
        }

        private IQueryable<CorrectiveAction> ApplyDueDateStatusFilter(IQueryable<CorrectiveAction> query)
        {
            if (DueDateStatus == null
                || DueDateStatus == DueDateStatusEnum.All)
            {
                return query;
            }
            else if (DueDateStatus == DueDateStatusEnum.DueSoon)
            {
                return query.Where(CorrectiveAction.Predicates.ActionStatuses.IsDueSoon);
            }
            else if (DueDateStatus == DueDateStatusEnum.PastDue)
            {
                return query.Where(CorrectiveAction.Predicates.ActionStatuses.IsDue);
            }

            throw new NotImplementedException(nameof(DueDateStatusEnum));
        }

        private static Expression<Func<CorrectiveAction, object>> GetOrderByExpression(string orderBy)
        {
            switch (orderBy)
            {
                case OrderByNameFieldName: return s => s.Name;
                case OrderByDueDateFieldName: return s => s.DueDate;
                case OrderByApprovalDateFieldName: return s => s.ApprovedAt;
                default: return s => s.Name;
            }
        }
    }
}