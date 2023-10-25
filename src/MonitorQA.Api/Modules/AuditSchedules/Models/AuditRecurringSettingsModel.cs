using System;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditSchedules.Models
{
    public class AuditRecurringSettingsModel
    {
        public AuditRepeatPattern RepeatPattern { get; set; }

        public int? RepeatEvery { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? EndAfterTimes { get; set; }
    }
}