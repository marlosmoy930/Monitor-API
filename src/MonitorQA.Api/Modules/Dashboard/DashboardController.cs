using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Dashboard
{
    [Route("dashboard")]
    [ApiController]
    public class DashboardController : AuthorizedController
    {
        private readonly SiteContext _context;

        public DashboardController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("audits/totals")]
        public async Task<IActionResult> GetAuditsTotals([FromQuery] DashboardRequestModel model)
        {
            var query = _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(Audit.Predicates.IsNotDeleted())
                .Where(Audit.Predicates.IsNotCompleted())
                .Where(a => a.AuditSchedule.TemplateId == model.TemplateId);

            if (model.AuditObjectId.HasValue)
                query = query.Where(a => a.AuditObjectId == model.AuditObjectId);

            var audits = await  query.ToListAsync();

            return Ok(new
            {
                Upcoming = audits.Count(Audit.Predicates.AuditStatuses.IsInFuture.Compile()),
                PastDue = audits.Count(Audit.Predicates.AuditStatuses.IsDue.Compile()),
                DueIn7Days = audits.Count(Audit.Predicates.AuditStatuses.IsDueSoon.Compile()),
                InProgress = audits.Count(Audit.Predicates.AuditStatuses.IsInProgress.Compile()),
                ReadyToStart = audits.Count(Audit.Predicates.AuditStatuses.IsReadyToStart.Compile())
            });
        }

        [HttpGet]
        [Route("audits/completion")]
        public async Task<IActionResult> GetAuditsCompletion([FromQuery] CompletedAuditsRequestModel model)
        {
            var query = _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(Audit.Predicates.IsNotDeleted())
                .Where(a => a.AuditSchedule.TemplateId == model.TemplateId)
                .Where(Audit.Predicates.AuditStatuses.IsCompleted)
                .Where(a => a.CompleteDate >= model.StartDate && a.CompleteDate <= model.EndDate);

            if (model.AuditObjectId.HasValue)
                query = query.Where(a => a.AuditObjectId == model.AuditObjectId);
            
            var audits = await query.ToListAsync();

            var completedAudits = audits.Where(Audit.Predicates.AuditStatuses.IsCompleted.Compile()).ToList();

            var scoreSystemElements = await _context.Templates.AsNoTracking()
                .Where(t => t.Id == model.TemplateId)
                .SelectMany(t => t.ScoreSystem.ScoreSystemElements)
                .ToListAsync();

            return Ok(new
            {
                AverageScore = completedAudits.Count > 0 ? completedAudits.Sum(a => a.Score) / completedAudits.Count : 0,
                Data = scoreSystemElements.Select(e => new
                {
                    e.Label,
                    e.Color,
                    ScoreMin = e.Min,
                    ScoreMax = e.Max,
                    Count = audits.Count(a => a.Score >= e.Min && a.Score <= e.Max)
                })
            });
        }

        [HttpGet]
        [Route("actions/statuses/totals")]
        public async Task<IActionResult> GetActionsTotalsByStatuses([FromQuery] DashboardRequestModel model)
        {
            var query = _context.CorrectiveActions
                .AsNoTracking()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(CorrectiveAction.Predicates.IsNotApproved())
                .Where(a => a.AuditItem.Audit.AuditSchedule.TemplateId == model.TemplateId);

            if (model.AuditObjectId.HasValue)
                query = query.Where(a => a.AuditItem.Audit.AuditObjectId == model.AuditObjectId);

            var actions = await query.ToListAsync();

            var open = actions.Where(a => a.Status == CorrectiveActionStatus.Open).ToList();
            var submitted = actions.Where(a => a.Status == CorrectiveActionStatus.Submitted).ToList();
            var rejected = actions.Where(a => a.Status == CorrectiveActionStatus.Rejected).ToList();

            return Ok(new
            {
                Open = new
                {
                    total = open.Count,
                    pastDue = open.Count(CorrectiveAction.Predicates.ActionStatuses.IsDue.Compile()),
                    dueIn7Days = open.Count(CorrectiveAction.Predicates.ActionStatuses.IsDueSoon.Compile()),
                },
                Submitted = new
                {
                    total = submitted.Count,
                    pastDue = submitted.Count(CorrectiveAction.Predicates.ActionStatuses.IsDue.Compile()),
                    dueIn7Days = submitted.Count(CorrectiveAction.Predicates.ActionStatuses.IsDueSoon.Compile()),
                },
                Rejected = new
                {
                    total = rejected.Count,
                    pastDue = rejected.Count(CorrectiveAction.Predicates.ActionStatuses.IsDue.Compile()),
                    dueIn7Days = rejected.Count(CorrectiveAction.Predicates.ActionStatuses.IsDueSoon.Compile()),
                }
            });
        }

        [HttpGet]
        [Route("actions/priorities/totals")]
        public async Task<IActionResult> GetActionsTotalsByPriority([FromQuery] DashboardRequestModel model)
        {
            var query = _context.CorrectiveActions
                .AsNoTracking()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(CorrectiveAction.Predicates.IsNotApproved())
                .Where(a => a.AuditItem.Audit.AuditSchedule.TemplateId == model.TemplateId);

            if (model.AuditObjectId.HasValue)
                query = query.Where(a => a.AuditItem.Audit.AuditObjectId == model.AuditObjectId);

            var actions = await query.ToListAsync();

            var low = actions.Where(a => a.Priority == CorrectiveActionPriority.Low).ToList();
            var medium = actions.Where(a => a.Priority == CorrectiveActionPriority.Medium).ToList();
            var high = actions.Where(a => a.Priority == CorrectiveActionPriority.High).ToList();

            return Ok(new
            {
                Low = new
                {
                    total = low.Count,
                    pastDue = low.Count(CorrectiveAction.Predicates.ActionStatuses.IsDue.Compile()),
                    dueIn7Days = low.Count(CorrectiveAction.Predicates.ActionStatuses.IsDueSoon.Compile()),
                },
                Medium = new
                {
                    total = medium.Count,
                    pastDue = medium.Count(CorrectiveAction.Predicates.ActionStatuses.IsDue.Compile()),
                    dueIn7Days = medium.Count(CorrectiveAction.Predicates.ActionStatuses.IsDueSoon.Compile()),
                },
                High = new
                {
                    total = high.Count,
                    pastDue = high.Count(CorrectiveAction.Predicates.ActionStatuses.IsDue.Compile()),
                    dueIn7Days = high.Count(CorrectiveAction.Predicates.ActionStatuses.IsDueSoon.Compile()),
                }
            });
        }
    }
}
