using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.AuditTemplates.Models
{
    public class ItemInformation
    {
        public string Text { get; set; }

        public IEnumerable<Guid> PhotosIds { get; set; }
    }
}