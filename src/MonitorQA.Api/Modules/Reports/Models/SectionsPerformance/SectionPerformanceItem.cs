using System;
using MonitorQA.Pdf.Reports.Executive.Models.SectionPerformance;

namespace MonitorQA.Api.Modules.Reports.Models.SectionsPerformance
{
    public class SectionPerformanceItem : PerformanceItem
    {
        public Guid TemplateItemId { get; set; }

        public string Name { get; set; }

        public override object Clone()
        {
            return new SectionPerformanceItem
            {
                TemplateItemId = TemplateItemId,
                Name = Name,
            };
        }

        internal PdfReportSectionPerformanceItem GetPdfReportSectionItem()
        {
            return new PdfReportSectionPerformanceItem
            {
                Name = Name,
                AverageScore = AverageScore,
                PreviousAverageScore = PreviousAverageScore,
                Count = Count,
            };
        }
    }
}
