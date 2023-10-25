using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditSchedules.Models
{
    public class AuditScheduleRequestModel
    {
        [NotEmpty]
        public Guid TemplateId { get; set; }

        public IEnumerable<Guid> AuditObjectIds { get; set; } = new List<Guid>();

        public IEnumerable<Guid> AuditObjectGroupIds { get; set; } = new List<Guid>();

        public string? Name { get; set; }

        public IEnumerable<Guid>? TagsIds { get; set; }

        public AuditRecurringSettingsModel Repeat { get; set; }

        public void UpdateScheduleEntity(AuditSchedule entity)
        {
            entity.Name = Name;
            entity.AuditScheduleAuditObjects = AuditObjectIds.Select(id => new AuditScheduleAuditObject
            {
                AuditObjectId = id
            }).ToList();
            entity.AuditScheduleAuditObjectGroups = AuditObjectGroupIds.Select(id => new AuditScheduleAuditObjectGroup
            {
                AuditObjectGroupId = id
            }).ToList();
            entity.TemplateId = TemplateId;

            entity.RepeatPattern = Repeat.RepeatPattern;
            entity.RepeatEvery = Repeat.RepeatEvery;
            entity.RepeatEndAfterTimes = Repeat.EndAfterTimes;
            entity.StartDate = Repeat.StartDate;
            entity.EndDate = Repeat.EndDate;

            if (TagsIds != null && TagsIds.Any())
            {
                entity.AuditScheduleTags = TagsIds.Select(id => new AuditScheduleTag
                {
                    TagId = id
                }).ToList();
            }
        }
    }
}
