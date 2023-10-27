using MonitorQA.Api.Infrastructure.Models;
using System;
using System.Collections.Generic;
using MonitorQA.Api.Modules.AuditTemplates.Models.TemplateItem.Response;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models
{
    public class GetTemplateModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public bool IsDraft { get; set; }

        public TemplateType TemplateType { get; set; }

        public bool? IsAuditorSignatureRequired { get; set; }
        
        public bool? IsAuditeeSignatureRequired { get; set; }

        public string? SignatureAgreement { get; set; }

        public IdNamePairModel<Guid> ScoreSystem { get; set; }

        public bool ConditionalTagsEnabled { get; set; }

        public IEnumerable<IdNamePairModel<Guid>> Tags { get; set; }

        public Dictionary<Guid, TemplateItemResponseModel> Data { get; set; }
    }
}