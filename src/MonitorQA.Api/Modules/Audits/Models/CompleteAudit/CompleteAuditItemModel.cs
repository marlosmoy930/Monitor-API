using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class CompleteAuditItemModel
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public ItemType ItemType { get; set; }

        public AnswerType? AnswerType { get; set; }

        public string? Text { get; set; }

        public string? Note { get; set; }

        public int Index { get; set; }

        public int? GroupIndex { get; set; }

        public bool NotApplicable { get; set; }

        public AuditItemSignatureInfo? Signature { get; set; }

        public IEnumerable<PhotoModel> Photos { get; set; } = new List<PhotoModel>();

        public List<AuditItemAnswerModel>? Data { get; set; } = new List<AuditItemAnswerModel>();

        public IEnumerable<CompleteAuditActionModel> Actions { get; set; } = new List<CompleteAuditActionModel>();

        public IEnumerable<ItemFlag> Flags { get; set; } = new List<ItemFlag>();

        public AuditItem GetAuditItem()
        {
            var item = new AuditItem
            {
                TemplateItemId = Id,
                ItemType = ItemType,
                AnswerType = AnswerType,
                Text = Text,
                Note = Note,
                Index = Index,
                GroupIndex = GroupIndex,
                NotApplicable = NotApplicable,

                Photos = Photos.Select(p => new AuditItemPhoto
                {
                    Id = p.Id,
                    Note = p.Note,
                    UpdatedAt = p.UpdatedAt
                }).ToList(),

                CorrectiveActions = Actions
                    .Select(a => a.GetCorrectiveAction())
                    .ToList(),

                Flags = Flags.Select(f => new AuditItemFlag
                {
                    Flag = f
                }).ToList()
            };

            if (Signature != null)
                Signature.UpdateEntity(item);

            var answers = Data.Select(a => new AuditItemAnswer
            {
                AuditItem = item,
                Selected = !NotApplicable && a.Selected,
                Points = a.Points,
                MarkAsFailed = a.MarkAsFailed,
                Name = a.Name,
                Color = a.Color,
                Index = a.Index,
                Number = a.Number,
                FromNumber = a.FromNumber,
                ToNumber = a.ToNumber,
                Text = a.Text
            }).ToList();

            item.Answers = answers;

            return item;
        }
    }
}