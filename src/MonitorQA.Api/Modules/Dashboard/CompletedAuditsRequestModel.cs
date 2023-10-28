using System;

namespace MonitorQA.Api.Modules.Dashboard
{
    public class CompletedAuditsRequestModel : DashboardRequestModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}