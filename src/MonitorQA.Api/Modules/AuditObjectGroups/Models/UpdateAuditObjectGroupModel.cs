using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorQA.Api.Modules.AuditObjectGroups.Models
{
    public class UpdateAuditObjectGroupModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Guid> AuditObjectIds { get; set; }

        public void UpdateEntity(AuditObjectGroup auditObjectGroup)
        {
            auditObjectGroup.Name = Name;
            auditObjectGroup.AuditObjectAuditObjectGroups = AuditObjectIds
                .Select(auditObjectId => new AuditObjectAuditObjectGroup
                {
                    AuditObjectGroupId = Id,
                    AuditObjectId = auditObjectId,
                }).ToList();
        }
    }
}