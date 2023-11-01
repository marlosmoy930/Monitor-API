using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.Reports.Models.CorrectiveActionsAnalysis
{
    public class CorrectiveActionStatusInfo
    {
        public Guid ActionId { get; set; }

        public CorrectiveActionStatus Status { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool IsLate
        {
            get
            {
                if (!DueDate.HasValue)
                    return false;

                if (ApprovedAt.HasValue)
                    return ApprovedAt.Value.Date.AddDays(1) > DueDate;

                return DateTime.UtcNow > DueDate;
            }
        }


    }
}
