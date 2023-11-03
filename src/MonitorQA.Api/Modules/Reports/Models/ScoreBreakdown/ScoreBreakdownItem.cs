using MonitorQA.Data.Entities;
using System;
using MonitorQA.Pdf.Reports.Executive.Models.ScoreBreakdown;

namespace MonitorQA.Api.Modules.Reports.Models.ScoreBreakdown
{

    public class ScoreBreakdownItem : CountableItem<decimal>
    {
        public string Label { get; set; }

        public string Color { get; set; }

        public int ScoreMin { get; set; }

        public int ScoreMax { get; set; }

        protected override Func<decimal, bool> TryAddPredicate =>
            score => ScoreMin <= score && score <= ScoreMax;

        public PdfReportScoreBreakdownItem ToPdfReportItem()
        {
            return new PdfReportScoreBreakdownItem { 
                Label = Label,
                Color = Color,
                ScoreMin = ScoreMin,
                ScoreMax = ScoreMax,
                Count = Count,
            };
        }

        public static ScoreBreakdownItem Create(ScoreSystemElement element)
        {
            var item = new ScoreBreakdownItem
            {
                Label = element.Label,
                Color = element.Color,
                ScoreMin = element.Min,
                ScoreMax = element.Max,
            };
            return item;
        }

    }
}