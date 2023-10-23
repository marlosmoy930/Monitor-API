using System;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.AuditList
{
    public class AuditScheduleResponseModel : IdNamePairModel<Guid>
    {
        public AuditRepeatPattern RepeatPattern { get; set; }

        public Guid TemplateId { get; set; }
    }
}