using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class AuditorSignatureInfo
    {
        public Guid PhotoId { get; set; }
        
        public Guid CreatedById { get; set; }
        
        public DateTime UpdatedAt { get; set; }

        internal void UpdateEntity(Audit audit)
        {
            audit.AuditorSignature = new AuditorSignature
            {
                AuditId = audit.Id,
                PhotoId = PhotoId,
                CreatedById = CreatedById,
                UpdatedAt = UpdatedAt,
            };
        }
    }
}