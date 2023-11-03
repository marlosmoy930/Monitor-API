using System;

namespace MonitorQA.Api.Modules.Reports.Models.ScoreBreakdown
{
    public class AuditScoreInfo
    {
        public Guid AuditId { get; set; }
        public decimal Score { get; set; }
    }
}