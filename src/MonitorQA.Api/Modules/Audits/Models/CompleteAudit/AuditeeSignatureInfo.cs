using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class AuditeeSignatureInfo
    {
        public Guid PhotoId { get; set; }

        public string CreatedByName { get; set; }

        public DateTime UpdatedAt { get; set; }

        internal void UpdateEntity(Audit audit)
        {
            audit.AuditeeSignature = new AuditeeSignature
            {
                AuditId = audit.Id,
                PhotoId = PhotoId,
                AuditeeName = CreatedByName,
                UpdatedAt = UpdatedAt,
            };
        }
    }
}