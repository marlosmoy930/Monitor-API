using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.AuditSchedules.Models
{
    public class AuditScheduleResponseModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public EntityConciseModel Template { get; set; }

        public AuditDateRangeModel CurrentAudit { get; set; }

        public RepeatSettingsModel Repeat { get; set; }

        public IEnumerable<EntityConciseModel> AuditObjects { get; set; }

        public IEnumerable<EntityConciseModel> AuditObjectGroups { get; set; }

        public IEnumerable<TagConciseModel> Tags { get; set; }
    }
}
