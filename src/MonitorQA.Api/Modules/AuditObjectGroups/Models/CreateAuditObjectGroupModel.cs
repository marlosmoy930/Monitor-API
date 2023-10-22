using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorQA.Api.Modules.AuditObjectGroups.Models
{
    public class CreateAuditObjectGroupModel
    {
        public string Name { get; set; }

        public IEnumerable<Guid> AuditObjectIds { get; set; }

        public AuditObjectGroup CreateEntity(Guid compnayId)
        {
            var auditObjectGroup = new AuditObjectGroup
            {
                Name = Name,
                CompanyId = compnayId
            };
            auditObjectGroup.AuditObjectAuditObjectGroups = AuditObjectIds
                .Select(auditObjectId => new AuditObjectAuditObjectGroup
                {
                    AuditObjectId = auditObjectId,
                }).ToList();

            return auditObjectGroup;
        }
    }
}