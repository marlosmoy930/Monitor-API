using MonitorQA.Data.Entities;
using MonitorQA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Pdf.Reports.Executive.Models;

namespace MonitorQA.Api.Modules.Reports.Models.CorrectiveActionsAnalysis
{
    public class ChartItem : CountableItem<CorrectiveActionStatus>
    {
        public CorrectiveActionStatus Status { get; set; }

        public string Label => Pdf.Audit.CorrectiveActionStatusInfo.GetLabelByStatus(Status);

        public string Color => Pdf.Audit.CorrectiveActionStatusInfo.GetColorByStatus(Status);

        protected override Func<CorrectiveActionStatus, bool> TryAddPredicate => 
            s => Status == s;

        public static List<ChartItem> CreateListByStatuses()
        {
            var items = GetCorrectiveActionStatuses()
                .Select(s => new ChartItem { Status = s })
                .ToList();
            return items;
        }

        public PdfReportChartItem ToPdfReportItem()
        {
            return new PdfReportChartItem
            {
                Count = Count,
                Status = Status,
            };
        }

        private static List<CorrectiveActionStatus> GetCorrectiveActionStatuses()
        {
            return new List<CorrectiveActionStatus> {
                CorrectiveActionStatus.Open,
                CorrectiveActionStatus.Approved,
                CorrectiveActionStatus.Rejected,
                CorrectiveActionStatus.Submitted
            };
        }
    }
}
