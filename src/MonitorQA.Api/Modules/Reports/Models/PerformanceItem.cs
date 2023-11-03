using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MonitorQA.Api.Modules.Reports.Models
{
    public abstract class PerformanceItem: ICloneable
    {
        [JsonIgnore]
        public readonly List<decimal> AuditScores = new List<decimal>();

        [JsonIgnore]
        public readonly List<decimal> PreviousAuditScores = new List<decimal>();

        public decimal? AverageScore => AuditScores.Any() ? (decimal?)AuditScores.Average() : null;

        public decimal? PreviousAverageScore => PreviousAuditScores.Any() ? (decimal?)PreviousAuditScores.Average() : null;

        public int Count => AuditScores.Count;
        
        public abstract object Clone();
    }
}
