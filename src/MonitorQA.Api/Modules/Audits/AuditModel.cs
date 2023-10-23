using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.Audits
{
    public class UpdateAuditRequestModel
    {
        public ICollection<Guid>? Assignees { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}