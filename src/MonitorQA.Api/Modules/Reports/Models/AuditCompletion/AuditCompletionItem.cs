using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MonitorQA.Api.Modules.Reports.Models.AuditCompletion
{
    public class AuditCompletionItem : ICloneable
    {
        public DateTime Date { get; set; }

        [JsonIgnore]
        public List<Audit> Audits { get; set; } = new List<Audit>();

        public int Count => Audits.Count;

        public object Clone()
        {
            return new AuditCompletionItem
            {
                Date = Date,
            };
        }
    }
}