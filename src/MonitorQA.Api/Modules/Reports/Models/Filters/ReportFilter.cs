using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Pdf.Reports.Executive.Models;

namespace MonitorQA.Api.Modules.Reports.Models.Filters
{
    public class ReportFilter
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Guid TemplateId { get; set; }

        public Guid? AuditObjectId { get; set; }

        public List<Guid>? UserIds { get; set; }

        public List<Guid>? UserGroupIds { get; set; }

        public List<Guid>? TagsIds { get; set; }

        public IQueryable<Audit> GetPendingAndCompletedAuditsQuery(IQueryable<Audit> auditsQuery, Guid companyId)
        {
            return GetFilteredAuditsQuery(auditsQuery, companyId)
                .Where(a => (a.IsCompleted && StartDate < a.CompleteDate && a.CompleteDate < EndDate)
                            || (!a.IsCompleted && a.StartDate < EndDate));
        }

        public IQueryable<Audit> GetFilteredCompletedAuditsQuery(IQueryable<Audit> auditsQuery, Guid companyId)
        {
            return GetFilteredAuditsQuery(auditsQuery, companyId)
                .Where(Audit.Predicates.IsCompleted())
                .Where(a => StartDate < a.CompleteDate && a.CompleteDate < EndDate);
        }

        public IQueryable<CorrectiveAction> GetFilteredActionsQuery(IQueryable<Audit> auditsQueryery, Guid companyId)
        {
            var auditsQuery = GetFilteredCompletedAuditsQuery(auditsQueryery, companyId);
            var actionsQuery = auditsQuery
                .SelectMany(a => a.AuditItems
                    .SelectMany(ai => ai.CorrectiveActions))
                .Where(CorrectiveAction.Predicates.IsNotDeleted());

            return actionsQuery;
        }

        public PdfReportFilter GetPdfReportFilter()
        {
            return new PdfReportFilter
            {
                StartDate = StartDate,
                EndDate = EndDate,
            };
        }

        private IQueryable<Audit> GetFilteredAuditsQuery(IQueryable<Audit> auditsQuery, Guid companyId)
        {
            auditsQuery = auditsQuery
                .Where(Audit.Predicates.BelongsToCompany(companyId))
                .Where(Audit.Predicates.IsNotDeleted());

            auditsQuery = auditsQuery.Where(a => a.AuditSchedule.TemplateId == TemplateId);

            if (AuditObjectId.HasValue)
                auditsQuery = auditsQuery.Where(a => a.AuditObjectId == AuditObjectId);

            if (UserIds != null && UserIds.Any())
                auditsQuery = auditsQuery
                    .Where(a => a.AuditObject.AuditObjectUsers.Any(aou => UserIds.Contains(aou.UserId)));

            if (UserGroupIds != null && UserGroupIds.Any())
                auditsQuery = auditsQuery
                    .Where(a => a.AuditObject.AuditObjectUsers
                        .Any(aou => aou.UserGroupId.HasValue && UserGroupIds.Contains(aou.UserGroupId.Value)));

            if (TagsIds != null && TagsIds.Any())
                auditsQuery = auditsQuery
                    .Where(a => a.AuditObject.AuditObjectTags.Any(aot => TagsIds.Contains(aot.TagId)));

            return auditsQuery;
        }
    }
}
