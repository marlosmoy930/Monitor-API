using System;

namespace MonitorQA.Api.Modules.Reports.Models.AuditDayOfWeekBreakdown
{
    public class AuditDayOfWeekBreakdownItem
    {
        public int Count { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public string DayOfWeekName => DayOfWeek.ToString();
    }
}
