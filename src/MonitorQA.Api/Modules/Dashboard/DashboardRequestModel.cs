using System;

namespace MonitorQA.Api.Modules.Dashboard
{
    public class DashboardRequestModel
    {
        public Guid TemplateId { get; set; }

        public Guid? AuditObjectId { get; set; }
    }
}