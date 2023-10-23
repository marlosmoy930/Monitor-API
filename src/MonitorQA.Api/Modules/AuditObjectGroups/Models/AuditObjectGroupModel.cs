using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.AuditObjectGroups.Models
{
    public class AuditObjectGroupModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Guid> AuditObjectIds { get; set; }
    }
}