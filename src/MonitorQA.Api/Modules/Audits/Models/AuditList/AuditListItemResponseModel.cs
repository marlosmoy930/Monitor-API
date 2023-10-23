using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.AuditList
{
    public class AuditListItemResponseModel
    {
        public Guid Id { get; set; }

        public string? Number { get; set; }

        public IdNamePairModel<Guid> AuditObject { get; set; }

        public IdNamePairModel<Guid> Template { get; set; }

        public AuditScheduleResponseModel AuditSchedule { get; set; }

        public IEnumerable<IdNamePairModel<Guid>> Assignees { get; set; }

        public IEnumerable<IdNamePairModel<Guid>> Notifees { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompleteDate { get; set; }

        public IdNamePairModel<Guid> CompletedBy { get; set; }

        public bool IsStarted { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int PendingActionsCount { get; set; }

        public int CompletedActionsCount { get; set; }

        public decimal Score { get; set; }

        public string ScoreLabel { get; set; }

        public IEnumerable<PreviousAuditResponseModel> PreviousAudits { get; set; }

        public static Expression<Func<Audit, AuditListItemResponseModel>> GetSelectExpression()
        {
            return a => new AuditListItemResponseModel
            {
                Id = a.Id,
                Number = a.Number,
                AuditObject = new IdNamePairModel<Guid>
                {
                    Id = a.AuditObject.Id,
                    Name = a.AuditObject.Name
                },
                Template = new IdNamePairModel<Guid>
                {
                    Id = a.AuditSchedule.Template.Id,
                    Name = a.AuditSchedule.Template.Name,
                },
                AuditSchedule = new AuditScheduleResponseModel
                {
                    Id = a.AuditSchedule.Id,
                    Name = a.AuditSchedule.Name,
                    RepeatPattern = a.AuditSchedule.RepeatPattern
                },
                Assignees = a.Assignees.Select(aa => new IdNamePairModel<Guid> { Id = aa.User.Id, Name = aa.User.Name }),
                Notifees = a.AuditObject.AuditObjectUsers.Select(aa => new IdNamePairModel<Guid> { Id = aa.User.Id, Name = aa.User.Name }).Distinct(),
                IsCompleted= a.IsCompleted,
                CompleteDate = a.CompleteDate,
                CompletedBy = new IdNamePairModel<Guid> { Id = a.CompletedBy.Id, Name = a.CompletedBy.Name },
                IsStarted  = a.IsStarted,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                CompletedActionsCount = a.AuditItems.SelectMany(ai => ai.CorrectiveActions.Where(ca => !ca.IsDeleted && ca.Status == CorrectiveActionStatus.Approved)).Count(),
                PendingActionsCount = a.AuditItems.SelectMany(ai => ai.CorrectiveActions.Where(ca => !ca.IsDeleted && ca.Status != CorrectiveActionStatus.Approved)).Count(),
                Score = a.Score,
                PreviousAudits = a.AuditObject.Audits
                    .Where(a2 => !a2.IsDeleted
                                 && a2.AuditObjectId == a.AuditObjectId
                                 && a2.AuditSchedule.TemplateId == a.AuditSchedule.TemplateId
                                 && a2.IsCompleted && a2.CompleteDate != null
                                 && (a.CompleteDate == null || a2.CompleteDate < a.CompleteDate))
                    .OrderByDescending(a2 => a2.CompleteDate)
                    .Select(a2 =>  new PreviousAuditResponseModel
                    {
                        Id = a2.Id,
                        Score = a2.Score,
                        StartedAt = a2.StartedAt,
                        CompleteDate = a2.CompleteDate
                    }).Take(1)
            };
        }

        public void SetScoreLabel(IReadOnlyCollection<ScoreSystemElement> scoreSystemElements)
        {
            ScoreLabel = ScoreSystemElement.GetLabelByScore(scoreSystemElements, Score);

            foreach (var prevAudit in PreviousAudits)
            {
                prevAudit.SetScoreLabel(scoreSystemElements);
            }
        }
    }
}