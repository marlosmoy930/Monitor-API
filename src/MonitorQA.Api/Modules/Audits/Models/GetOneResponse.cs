using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Api.Modules.Audits.Models.AuditList;

namespace MonitorQA.Api.Modules.Audits.Models
{
    public class GetOneResponse
    {
        public Guid Id { get; set; }

        public IdNamePairModel<Guid?>? AuditObject { get; set; }

        public IdNamePairModel<Guid?>? StartedBy { get; set; }

        public TemplateInfo Template { get; set; }

        public AuditScheduleResponseModel? AuditSchedule { get; set; }

        public IEnumerable<IdNamePairModel<Guid?>>? Assignees { get; set; }

        public IEnumerable<IdNamePairModel<Guid?>>? Notifees { get; set; }

        public bool IsStarted { get; set; }

        public DateTime? StartedAt { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompleteDate { get; set; }

        public IdNamePairModel<Guid?> CompletedBy { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ActionsCount { get; set; }

        public decimal Score { get; set; }

        public bool? CanBeReopened { get; set; }

        public SignatureInfo AuditorSignature { get; set; }

        public SignatureInfo AuditeeSignature { get; set; }

        public IEnumerable<PreviousAudit> PreviousAudits { get; set; }

        public string? ScoreLabel { get; set; }

        public double? CompletionTime { get; set; }

        public Progress? Progress { get; set; }

        public static Expression<Func<Audit, GetOneResponse>> GetSelectExpression()
        {
            return a => new GetOneResponse
            {
                Id = a.Id,
                AuditObject = new IdNamePairModel<Guid?>
                {
                    Id = a.AuditObjectId,
                    Name = a.AuditObject != null ? a.AuditObject.Name : null
                },
                StartedBy = new IdNamePairModel<Guid?>
                {
                    Id = a.StartedById,
                    Name = a.StartedById.HasValue
                        ? a.StartedBy.Name
                        : null
                },
                CompletedBy = new IdNamePairModel<Guid?>
                {
                    Id = a.CompletedById,
                    Name = a.CompletedById.HasValue
                        ? a.CompletedBy.Name
                        : null
                },
                AuditSchedule = a.AuditScheduleId.HasValue ? new AuditScheduleResponseModel
                {
                    Id = a.AuditSchedule.Id,
                    Name = a.AuditSchedule.Name,
                    RepeatPattern = a.AuditSchedule.RepeatPattern,
                    TemplateId = a.AuditSchedule.TemplateId
                } : null,
                Assignees = a.Assignees.Select(aa => new IdNamePairModel<Guid?>
                {
                    Id = aa.User.Id,
                    Name = aa.User.Name
                }),
                Notifees = a.AuditObject.AuditObjectUsers.Select(aa => new IdNamePairModel<Guid?>
                {
                    Id = aa.User.Id,
                    Name = aa.User.Name
                }).Distinct(),
                IsStarted = a.IsStarted,
                StartedAt = a.StartedAt,
                IsCompleted = a.IsCompleted,
                CompleteDate = a.CompleteDate,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                ActionsCount = a.AuditItems.SelectMany(i => i.CorrectiveActions).Count(),
                Score = a.Score,
                AuditorSignature = a.AuditorSignature != null
                    ? new SignatureInfo
                    {
                        PhotoId = a.AuditorSignature.PhotoId,
                        CreatedBy = new IdNamePairModel<Guid> { 
                            Id = a.AuditorSignature.CreatedById,
                            Name  = a.AuditorSignature.CreatedBy.Name
                        },
                        CreatedByName = a.AuditorSignature.CreatedBy.Name,
                        UpdatedAt = a.AuditorSignature.UpdatedAt
                    }
                    : null,
                AuditeeSignature = a.AuditeeSignature != null
                    ? new SignatureInfo
                    {
                        PhotoId = a.AuditeeSignature.PhotoId,
                        CreatedByName = a.AuditeeSignature.AuditeeName,
                        UpdatedAt = a.AuditorSignature.UpdatedAt
                    }
                    : null,
                PreviousAudits = a.AuditObject.Audits
                        .Where(a2 => !a2.IsDeleted
                                     && a2.AuditObjectId == a.AuditObjectId
                                     && a2.AuditSchedule.TemplateId == a.AuditSchedule.TemplateId
                                     && a2.IsCompleted && a2.CompleteDate != null
                                     && (!a.CompleteDate.HasValue || a2.CompleteDate < a.CompleteDate))
                        .OrderByDescending(a2 => a2.CompleteDate)
                        .Select(a2 => new PreviousAudit
                        {
                            Id = a2.Id,
                            Score = a2.Score,
                            StartedAt = a2.StartedAt,
                            CompleteDate = a2.CompleteDate,
                        }).Take(1),
                CanBeReopened = a.IsCompleted
                                && !a.AuditObject.Audits
                                    .Any(a2 => !a2.IsDeleted
                                               && a2.AuditObjectId == a.AuditObjectId
                                               && a2.AuditSchedule.TemplateId == a.AuditSchedule.TemplateId
                                               && a2.IsCompleted && a2.CompleteDate != null
                                               && (a2.CompleteDate > a.CompleteDate || a2.StartedAt > a.CompleteDate))
            };

        }

        public void AddCompletionData(
            IEnumerable<ScoreSystemElement> scoreSystemElements,
            List<AuditItem> auditItems)
        {
            var rootItem = auditItems.FirstOrDefault(ai => ai.ItemType == ItemType.Root);
            rootItem.InitTree(auditItems);

            ScoreLabel = ScoreSystemElement.GetLabelByScore(scoreSystemElements, Score);

            if (CompleteDate.HasValue)
            {
                if (StartedAt.HasValue)
                {
                    var completionTime = CompleteDate.Value - StartedAt.Value;
                    CompletionTime = completionTime.TotalSeconds;
                }
            }

            Progress = Progress.Create(rootItem);
        }
    }

    public class TemplateInfo
    {
        public Guid Id { get; set; }

        public decimal? PassedThreshold { get; set; }

        public bool? IsAuditorSignatureRequired { get; set; }

        public bool? IsAuditeeSignatureRequired { get; set; }

        public string? SignatureAgreement { get; set; }
    }

    public class Progress
    {
        public int CompletedSections { get; set; }

        public int AllSections { get; set; }

        public int CompletedItems { get; set; }

        public int AllItems { get; set; }

        internal static Progress? Create(AuditItem rootItem)
        {
            if (rootItem == null) return null;

            var progress = new Progress
            {
                AllItems = rootItem.GetAnsweredItemsCount(),
                CompletedItems = rootItem.GetAnsweredItemsCount(),
                AllSections = rootItem.GetSectionsCount(),
                CompletedSections = rootItem.GetSuccessSectionsCount(),
            };

            return progress;
        }
    }

    public class PreviousAudit
    {
        public Guid Id { get; set; }

        public string ScoreLabel { get; set; }

        public decimal Score { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompleteDate { get; set; }
    }

    public class SignatureInfo
    {
        public Guid PhotoId { get; set; }

        [Obsolete]
        public IdNamePairModel<Guid> CreatedBy { get; set; }
        
        public string CreatedByName { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
