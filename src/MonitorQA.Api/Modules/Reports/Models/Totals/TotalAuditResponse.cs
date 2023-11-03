using MonitorQA.Pdf.Reports.Executive.Models.Totals;

namespace MonitorQA.Api.Modules.Reports.Models.Totals
{
    public class TotalAuditResponse
    {
        public int Total { get; set; }
        
        public int Completed { get; set; }

        public int PreviousTotal { get; set; }

        public int PreviousCompleted { get; set; }

        internal PdfReportTotalAudit GetPdfReportTotalAudit()
        {
            return new PdfReportTotalAudit { 
                Total = Total,
                Completed = Completed,
                PreviousTotal = PreviousTotal,
                PreviousCompleted = PreviousCompleted,
            };
        }
    }

}
