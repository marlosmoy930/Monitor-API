using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class AuditItemSignatureInfo
    {
        public Guid PhotoId { get; set; }

        public IdNamePairModel<Guid> CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        internal void UpdateEntity(AuditItem item)
        {
            item.AuditItemSignature = new AuditItemSignature
            {
                PhotoId = PhotoId,
                CreatedById = CreatedBy.Id,
                UpdatedAt = UpdatedAt,
            };
        }
    }
}