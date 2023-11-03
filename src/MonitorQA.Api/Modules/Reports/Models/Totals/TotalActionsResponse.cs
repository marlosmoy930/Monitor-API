using MonitorQA.Data.Entities;
using System.Linq;
using MonitorQA.Pdf.Reports.Executive.Models.Totals;

namespace MonitorQA.Api.Modules.Reports.Models.Totals
{
    public class TotalActionsResponse
    {
        public int Total { get; set; }

        public int Approved { get; set; }

        public int PreviousTotal { get; set; }

        public int PreviousApproved { get; set; }

        public static IQueryable<CorrectiveAction> GetActionsQuery(IQueryable<Audit> filteredAuditsQuery)
        {
            var query = filteredAuditsQuery
                .SelectMany(a => a.AuditItems
                    .SelectMany(ai => ai.CorrectiveActions))
                .Where(CorrectiveAction.Predicates.IsNotDeleted());

            return query;
        }

        internal PdfReportTotalActions GetPdfReportTotalActions()
        {
            return new PdfReportTotalActions { 
                Total = Total,
                Approved = Approved,
                PreviousTotal = PreviousTotal,
                PreviousApproved = PreviousApproved,
            };
        }

        public static IQueryable<CorrectiveAction> GetAprrovedActionsQuery(IQueryable<CorrectiveAction> filteredActionsQuery)
        {
            return filteredActionsQuery.Where(CorrectiveAction.Predicates.IsApproved());
        }
    }

}
