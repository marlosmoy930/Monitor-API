using MonitorQA.Pdf.Reports.Executive.Models.Totals;

namespace MonitorQA.Api.Modules.Reports.Models.Totals
{
    public class TotalsResponse
    {
        public TotalAuditResponse Audits { get; set; }
        public TotalActionsResponse Actions { get; set; }

        public PdfReportTotals GetPdfReportTotals()
        {
            return new PdfReportTotals
            {
                Audits = Audits.GetPdfReportTotalAudit(),
                Actions = Actions.GetPdfReportTotalActions(),
            };
        }
    }
}
