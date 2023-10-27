using System;
using System.Collections.Generic;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models
{
    public class TemplateModel
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public IEnumerable<Guid>? TagsIds { get; set; }

        public bool IsDraft { get; set; }

        public TemplateType TemplateType { get; set; }

        public bool? IsAuditorSignatureRequired { get; set; }
        
        public bool? IsAuditeeSignatureRequired { get; set; }

        public string? SignatureAgreement { get; set; }

        public Guid? ScoreSystemId { get; set; }
    }
}