using System;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Dashboard
{
    public class AuditDataModel : IHasAuditStatus
    {
        public decimal Score { get; set; }

        public Guid AuditObjectId { get; set; }

        public string AuditObjectName { get; set; }

        public bool IsStarted { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompleteDate { get; set; }
    }
}