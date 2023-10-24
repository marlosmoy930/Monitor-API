using System;
using System.Collections.Generic;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.AuditList
{
    public class PreviousAuditResponseModel
    {
        public Guid Id { get; set; }

        public decimal Score { get; set; }

        public string ScoreLabel { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompleteDate { get; set; }

        public void SetScoreLabel(IReadOnlyCollection<ScoreSystemElement> scoreSystemElements)
        {
            ScoreLabel = ScoreSystemElement.GetLabelByScore(scoreSystemElements, Score);
        }
    }
}