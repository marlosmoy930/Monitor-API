using System;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Dashboard
{
    public class CorrectiveActionDataModel
    {
        public CorrectiveActionDataModel(CorrectiveAction entity)
        {
            DueDate = entity.DueDate;
            Status = entity.Status;
        }

        public DateTime? DueDate { get; set; }

        public CorrectiveActionStatus Status { get; set; }
    }
}