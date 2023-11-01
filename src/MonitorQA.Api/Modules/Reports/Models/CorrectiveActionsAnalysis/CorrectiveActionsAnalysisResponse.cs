using System.Collections.Generic;
using System.Linq;
using MonitorQA.Pdf.Reports.Executive.Models.CorrectiveActionsAnalysis;

namespace MonitorQA.Api.Modules.Reports.Models.CorrectiveActionsAnalysis
{
    public class CorrectiveActionsAnalysisResponse
    {
        public List<ChartItem> Total { get; set; } = ChartItem.CreateListByStatuses();

        public List<ChartItem> Late { get; set; } = ChartItem.CreateListByStatuses();

        public PdfReportCorrectiveActionsAnalysis GetPdfReportCorrectiveActionsAnalysis(string? svgTotal, string? svgLate)
        {
            var model = new PdfReportCorrectiveActionsAnalysis
            {
                SvgTotal = svgTotal,
                SvgLate = svgLate,
                Total = Total.Select(i => i.ToPdfReportItem()).ToList(),
                Late = Late.Select(i => i.ToPdfReportItem()).ToList(),
            };

            return model;
        }
    }
}
