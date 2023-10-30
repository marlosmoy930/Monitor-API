using MonitorQA.Pdf.Reports.Executive.Models.AuditCompliance;

namespace MonitorQA.Api.Modules.Reports.Models
{
    public class AuditComplianceResponse
    {
        public decimal Score { get; set; }

        public decimal PreviousScore { get; set; }

        public string ScoreColor { get; set; }

        internal PdfReportAuditCompliance GetPdfReportAuditCompliance()
        {
            return new PdfReportAuditCompliance {
                Score = Score,
                PreviousScore = PreviousScore,
                ScoreColor = ScoreColor,
            };
        }
    }
}