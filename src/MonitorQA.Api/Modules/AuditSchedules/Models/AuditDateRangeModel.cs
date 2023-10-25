using System;

namespace MonitorQA.Api.Modules.AuditSchedules.Models
{
    public class AuditDateRangeModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}