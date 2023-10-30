using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.Reports.Models.AuditPerformance
{
    public class AuditPerformanceInfo
    {
        public DateTime CompleteDate { get; set; }

        public decimal Score { get; set; }

        public IEnumerable<decimal> PreviousAuditScores { get; set; }
    }
}