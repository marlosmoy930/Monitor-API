using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MonitorQA.Api.Modules.Reports.Models.AuditPerformance
{
    public class AuditPerformanceItem : PerformanceItem
    {
        [JsonIgnore]
        public List<AuditInfoItem>? AuditInfoItems { get; set; }

        public DateTime Date { get; set; }

        public override object Clone()
        {
            return new AuditPerformanceItem
            {
                Date = Date,
            };
        }
    }
}