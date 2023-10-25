using System;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditSchedules.Models.AdHoc
{
    public class AdHocAuditModel
    {
        public string? Name { get; set; }

        public DateTime EndDate { get; set; }

        [NotEmpty]
        public Guid TemplateId { get; set; }

        [NotEmpty]
        public Guid AuditObjectId { get; set; }

        public void UpdateScheduleEntity(AuditSchedule entity)
        {
            entity.Name = Name;
            entity.TemplateId = TemplateId;

            entity.RepeatPattern = AuditRepeatPattern.OneTime;
            entity.StartDate = DateTime.UtcNow;
            entity.EndDate = EndDate;
        }
    }
}
