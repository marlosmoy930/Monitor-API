using System;

namespace MonitorQA.Api.Modules.Reports.Models.AuditDayOfWeekBreakdown
{
    public class AuditDayOfWeekBreakdownInfo
    {
        public Guid AuditId { get; set; }

        public DateTime CompleteDate { get; set; }

        public DayOfWeek DayOfWeek => CompleteDate.DayOfWeek;
    }
}
