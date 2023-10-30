using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.Reports.Models
{
    public class AuditInfoItem
    {
        public Audit Audit { get; set; }
        
        public Guid? PreviousAuditId { get; set; }
    }
}
