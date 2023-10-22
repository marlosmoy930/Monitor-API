using System.Collections.Generic;

namespace MonitorQA.Api.Modules.AuditObjectGroups.Models
{
    public class AuditObjectUserDetailsModel
    {
        public int AvailableAuditObjectsCount { get; set; }

        public int UngroupedAuditObjectsCount { get; set; }

        public IEnumerable<AuditObjectGroupModel> Groups { get; set; }
    }
}