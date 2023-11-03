using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.Reports.Models.SectionsPerformance;
using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Pdf.Reports.Executive.Models.SectionPerformance;

namespace MonitorQA.Api.Modules.Reports.Models
{
    public class PerformanceResponse<T> where T : ICloneable
    {
        public List<T> Items { get; set; } = new List<T>();

        public List<CompareItem<T>> Compare { get; set; } = new List<CompareItem<T>>();

        internal PdfReportPerformance<PdfReportSectionPerformanceItem> GetPdfReportSectionPermance(List<IdNamePairModel<Guid>> idNamePairs)
        {
            return new PdfReportPerformance<PdfReportSectionPerformanceItem>
            {
                Items = Items
                    .Select(i => (i as SectionPerformanceItem)!.GetPdfReportSectionItem())
                    .ToList(),
                Compare = Compare
                    .Select(i => (i as CompareItem<SectionPerformanceItem>)!.GetPdfReportSectionItem(idNamePairs))
                    .ToList(),
            };
        }
    }
}
