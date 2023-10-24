using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class CompleteAuditModel
    {
        public DateTime? StartedAt { get; set; }

        public DateTime? CompleteDate { get; set; }

        public AuditorSignatureInfo? AuditorSignature { get; set; }
        
        public AuditeeSignatureInfo? AuditeeSignature { get; set; }

        public Dictionary<Guid, CompleteAuditItemModel> Items { get; set; }

        public void UpdateEntity(User currentUser, Audit audit)
        {
            var items = Items.Values;

            audit.IsStarted = true;
            audit.StartedAt = StartedAt;

            if (!audit.StartedById.HasValue)
            {
                audit.StartedById = currentUser.Id;
            }

            AuditorSignature?.UpdateEntity(audit);

            AuditeeSignature?.UpdateEntity(audit);

            audit.IsCompleted = true;
            audit.CompleteDate = CompleteDate;
            audit.CompletedById = currentUser.Id;

            if (audit.Assignees.All(au => au.UserId != currentUser.Id))
            {
                var currentUserAssignee = new AuditAssignee
                {
                    UserId = currentUser.Id,
                    AuditId = audit.Id,
                };
                audit.Assignees.Add(currentUserAssignee);
            }

            audit.AuditItems = items
                .Select(i => i.GetAuditItem())
                .ToList();

            foreach (var auditItem in audit.AuditItems)
            {
                var templateItem = items.FirstOrDefault(ti => ti.Id == auditItem.TemplateItemId);
                var parentTemplateItem = items.FirstOrDefault(ti => ti.Id == templateItem.ParentId);

                if (parentTemplateItem != null)
                {
                    var parentAuditItem =
                        audit.AuditItems.FirstOrDefault(ai => ai.TemplateItemId == parentTemplateItem.Id);
                    auditItem.Parent = parentAuditItem;
                }
            }
        }
    }
}