using MonitorQA.Data.Entities;
using System.Linq;

namespace MonitorQA.Api.Modules.Reports
{
    public static class ReportsUtil
    {
        public static IQueryable<Audit> GetPreviousAuditsQuery(IQueryable<Audit> filteredAuditsQuery)
        {
            var query = filteredAuditsQuery
                .SelectMany(a => a.AuditObject.Audits
                        .Where(a2 => !a2.IsDeleted)
                        .Where(a2 => a2.AuditObjectId == a.AuditObjectId)
                        .Where(a2 => a2.AuditSchedule.TemplateId == a.AuditSchedule.TemplateId)
                        .Where(a2 => a2.IsCompleted)
                        .Where(a2 => !a.CompleteDate.HasValue || a2.CompleteDate < a.CompleteDate)
                        .OrderByDescending(a2 => a2.CompleteDate)
                        .Take(1));

            return query;
        }

        public static IQueryable<Audit> GetPreviousCompletedAuditsQuery(IQueryable<Audit> filteredAuditsQuery)
        {
            var query = GetPreviousAuditsQuery(filteredAuditsQuery)
                .Where(Audit.Predicates.IsCompleted());

            return query;
        }
    }
}
