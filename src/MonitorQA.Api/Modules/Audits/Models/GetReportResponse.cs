using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorQA.Api.Modules.Audits.Models
{
    public class GetReportResponse
    {
        public string Id { get; }

        public List<ReportItemModel> Items { get; }

        public GetReportResponse(
            Audit audit,
            ScoreSystem scoreSystem,
            Audit? previousAudit)
        {
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            Id = audit.Id.ToString();
            Items = audit.AuditItems
                .OrderBy(i => i.Index)
                .Select(i => new ReportItemModel(i, scoreSystem, previousAudit))
                .ToList();
        }
    }

    public class ReportItemModel
    {
        public Guid Id { get; }

        public Guid TemplateItemId { get; }

        public string Text { get; }

        public ItemType ItemType { get; }

        public AnswerType? AnswerType { get; }

        public string Note { get; }

        public int Index { get; }

        public int? GroupIndex { get; }

        public bool NotApplicable { get; set; }

        public bool? IsPassed { get; set; }

        public List<Guid> ChildrenIds { get; }

        public List<ReportAuditItemAnswer> Data { get; }

        public List<ReportCorrectiveAction> Actions { get; }

        public List<ReportAuditItemPhoto> Photos { get; }

        public List<ReportItemFlagInfo> Flags { get; }

        public int? ItemsCount { get; set; }

        public PointsInfo? Points { get; set; }

        public ReportAuditItemSignature? Signature { get; set; }

        public ReportItemModel(
            AuditItem auditItem,
            ScoreSystem scoreSystem,
            Audit? previousAudit)
        {
            if (auditItem is null) throw new ArgumentNullException(nameof(auditItem));

            if (auditItem.ItemType == ItemType.Root || auditItem.ItemType == ItemType.Section)
            {
                IsPassed = auditItem.GetIsPassed(scoreSystem.PassedThreshold);
                ItemsCount = auditItem.GetAllItemsCount();

                var score = auditItem.GetScore();
                var previousItem = previousAudit?.AuditItems.SingleOrDefault(i => i.TemplateItemId == auditItem.TemplateItemId);

                Points = new PointsInfo
                {
                    Total = auditItem.GetBestAnswersPoints(),
                    Selected = auditItem.GetSelectedAnswersPoints(),
                    Score = score,
                    Color = ScoreSystemElement.GetElementByScore(scoreSystem.ScoreSystemElements, score).Color,
                    PreviousScore = previousItem?.GetScore(),
                };
            }

            List<Guid> childrenIds;
            if (auditItem.ItemType == ItemType.ConditionalItem)
            {
                childrenIds = auditItem.Parent.Children
                    .Where(x => x.Id != auditItem.Id)
                    .Where(x => x.GroupIndex.HasValue)
                    .Where(x => x.GroupIndex == auditItem.Index)
                    .OrderBy(x => x.GetOrderIndex())
                    .Select(x => x.Id)
                    .ToList();
            }
            else
            {
                childrenIds = auditItem.Children
                            .Where(x => !x.GroupIndex.HasValue)
                            .OrderBy(x => x.GetOrderIndex())
                            .Select(x => x.Id)
                            .ToList();
            }

            var answers = auditItem.Answers
                ?.Select(ReportAuditItemAnswer.Create)
                .OrderBy(x => x.Index)
                .ToList()
                ?? new List<ReportAuditItemAnswer>();

            Id = auditItem.Id;
            TemplateItemId = auditItem.TemplateItemId;
            Text = auditItem.Text;
            ItemType = auditItem.ItemType;
            AnswerType = auditItem.AnswerType;
            Note = auditItem.Note;
            Index = auditItem.Index;
            GroupIndex = auditItem.GroupIndex;
            NotApplicable = auditItem.NotApplicable;
            ChildrenIds = childrenIds;
            Data = answers;
            Photos = auditItem.Photos?.Select(ReportAuditItemPhoto.Create).ToList()
                ?? new List<ReportAuditItemPhoto>();
            Flags = auditItem.Flags?.Select(ReportItemFlagInfo.Create).ToList()
                ?? new List<ReportItemFlagInfo>();
            Actions = auditItem.CorrectiveActions
                .Where(a => !a.IsDeleted)
                .Select(a => new ReportCorrectiveAction(a))
                .ToList();
            Signature = ReportAuditItemSignature.Create(auditItem);
        }
    }

    public class PointsInfo
    {
        public int Total { get; set; }

        public int Selected { get; set; }

        public decimal Score { get; set; }

        public decimal? PreviousScore { get; set; }

        public string Color { get; set; }

    }

    public class ReportAuditItemPhoto
    {
        public Guid PhotoId { get; set; }

        public Guid AuditItemId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string Note { get; set; }

        public static ReportAuditItemPhoto Create(AuditItemPhoto photo)
        {
            return new ReportAuditItemPhoto
            {
                PhotoId = photo.Id,
                AuditItemId = photo.AuditItemId,
                UpdatedAt = photo.UpdatedAt,
                Note = photo.Note,
            };
        }
    }

    public class ReportAuditItemSignature
    {
        public Guid PhotoId { get; set; }

        public Guid AuditId { get; set; }

        public IdNamePairModel<Guid>? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public static ReportAuditItemSignature? Create(AuditItem auditItem)
        {
            var signature = auditItem.AuditItemSignature;
            if (signature == null) return null;

            return new ReportAuditItemSignature
            {
                PhotoId = signature.PhotoId,
                AuditId = auditItem.AuditId,
                UpdatedAt = signature.UpdatedAt,
                CreatedBy = new IdNamePairModel<Guid> { Id = signature.CreatedById, Name = signature.CreatedBy.Name },
            };
        }
    }

    public class ReportAuditItemAnswer
    {
        public Guid Id { get; set; }

        public Guid AuditItemId { get; set; }

        public bool Selected { get; set; }

        public decimal? Number { get; set; }

        public int? Points { get; set; }

        public bool MarkAsFailed { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public int Index { get; set; }

        public static ReportAuditItemAnswer Create(AuditItemAnswer answer)
        {
            return new ReportAuditItemAnswer
            {
                Id = answer.Id,
                AuditItemId = answer.AuditItemId,
                Selected = answer.Selected,
                Points = answer.Points,
                MarkAsFailed = answer.MarkAsFailed,
                Name = answer.Name,
                Color = answer.Color,
                Index = answer.Index,
                Number = answer.Number
            };
        }
    }

    public class ReportCorrectiveAction
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<ReportCorrectiveActionComment> Comments { get; set; }

        public IdNamePairModel<Guid> CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public CorrectiveActionPriority Priority { get; set; }

        public CorrectiveActionStatus Status { get; set; }

        public IEnumerable<PhotoModel> Photos { get; set; }


        public ReportCorrectiveAction(CorrectiveAction action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));


            Id = action.Id;
            Name = action.Name;
            Description = action.Description;
            Photos = action.Photos.Select(p => new PhotoModel
            {
                Id = p.Id,
                UpdatedAt = p.UpdatedAt,
                Note = p.Note
            });
            Comments = action.Comments.Select(com => new ReportCorrectiveActionComment
            {
                Id = com.Id,
                CreatedBy = new IdNamePairModel<Guid>
                {
                    Id = com.CreatedBy.Id,
                    Name = com.CreatedBy.Name,
                },
                CreatedAt = com.CreatedAt,
                Body = com.Body,
                Photos = com.Photos.Select(p => new PhotoModel
                {
                    Id = p.Id,
                    UpdatedAt = p.UpdatedAt,
                    Note = p.Note
                })
            });
            CreatedBy = new IdNamePairModel<Guid>
            {
                Id = action.CreatedById,
                Name = action.CreatedBy.Name
            };
            CreatedAt = action.CreatedAt;
            DueDate = action.DueDate;
            Priority = action.Priority;
            Status = action.Status;
        }
    }

    public class ReportCorrectiveActionComment
    {
        public Guid Id { get; set; }

        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }

        public IdNamePairModel<Guid> CreatedBy { get; set; }

        public IEnumerable<PhotoModel> Photos { get; set; }
    }

    public class ReportItemFlagInfo
    {
        public ItemFlag Flag { get; set; }

        public string Name { get; set; }

        public static ReportItemFlagInfo Create(AuditItemFlag flag)
        {
            return new ReportItemFlagInfo
            {
                Flag = flag.Flag,
                Name = flag.Name,
            };
        }
    }
}
