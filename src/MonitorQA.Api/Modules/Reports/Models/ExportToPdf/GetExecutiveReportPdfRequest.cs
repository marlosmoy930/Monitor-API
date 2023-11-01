using MonitorQA.Api.Modules.Reports.Models.Filters;
using System;

namespace MonitorQA.Api.Modules.Reports.Models.ExportToPdf
{
    public class GetExecutiveReportPdfRequest
    {
        public DateTime? CreatedAt { get; set; }

        public ReportFilter Filter { get; set; } = new ReportFilter();

        public ReportCompare? SectionPerformanceCompare { get; set; }

        public string? ScoreBreakdownSvg { get; set; }

        public string? CorrectiveActionsAnalysisTotalSvg { get; set; }

        public string? CorrectiveActionsAnalysisLateSvg { get; set; }

        public string? AuditPerformanceSvg { get; set; }

        public string? AuditCompletionSvg { get; set; }

        public string? AuditDayOfWeekBreakdownSvg { get; set; }

        public string? AuditCompletionTimeSvg { get; set; }
    }
}
