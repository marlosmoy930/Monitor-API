using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.Audits.Models;
using MonitorQA.Api.Modules.Audits.Models.AuditList;
using MonitorQA.Api.Modules.Audits.Models.CompleteAudit;
using MonitorQA.AuditResults;
using MonitorQA.Api.Modules.AuditTemplates.Models;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Domain;
using MonitorQA.Notifications.Audits;
using MonitorQA.Notifications.CorrectiveActions;
using MonitorQA.Pdf.Audit.Details;

namespace MonitorQA.Api.Modules.Audits
{

    [Route("audit/all")]
    [ApiController]
    public class AuditsController : AuthorizedController
    {
        private readonly CloudMessagePublisher _publisher;
        private readonly SiteContext _context;
        private readonly AuditDetailsReportBuilder _auditDetailsReportBuilder;

        public AuditsController(
            CloudMessagePublisher publisher,
            SiteContext context,
            AuditDetailsReportBuilder auditDetailsReportBuilder) : base(context)
        {
            _publisher = publisher;
            _context = context;
            _auditDetailsReportBuilder = auditDetailsReportBuilder;
        }

        [HttpGet]
        [Route("upcoming")]
        [ProducesResponseType(typeof(ListPageResult<AuditListItemResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpcomingAudits([FromQuery] UpcomingAuditListRequestModel model)
        {
            var role = CurrentUser.Role;
            if (!role.CanDoAudits)
                return Forbid();

            var query = _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(Audit.Predicates.IsNotDeleted())
                .Where(Audit.Predicates.IsNotCompleted());

            query = model.ApplyFilters(query, CurrentUser, role);
            var count = await query.CountAsync();
            query = model.ApplyOrdering(query);

            var auditItems = await query.Select(AuditListItemResponseModel.GetSelectExpression())
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .ToListAsync();

            var scoreSystemElements = await _context.ScoreSystemElements.AsNoTracking()
                .Where(s => s.ScoreSystem.CompanyId == CurrentUser.CompanyId)
                .ToListAsync();

            foreach (var auditItem in auditItems)
                auditItem.SetScoreLabel(scoreSystemElements);

            return Ok(ListPageResult<AuditListItemResponseModel>.Create(auditItems, count, model));
        }

        [HttpGet]
        [Route("complete")]
        [ProducesResponseType(typeof(ListPageResult<AuditListItemResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompleteAudits([FromQuery] CompleteAuditListRequestModel model)
        {
            var role = CurrentUser.Role;
            if (!role.CanViewAuditsResults)
                return Forbid();

            var query = _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(a => !a.IsDeleted)
                .Where(Audit.Predicates.IsCompleted());

            query = model.ApplyFilters(query, CurrentUser);
            var count = await query.CountAsync();
            query = model.ApplyOrdering(query);

            var auditItems = await query.Select(AuditListItemResponseModel.GetSelectExpression())
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .ToListAsync();

            var scoreSystemElements = await _context.ScoreSystemElements.AsNoTracking()
                .Where(s => s.ScoreSystem.CompanyId == CurrentUser.CompanyId)
                .ToListAsync();

            foreach (var auditModel in auditItems)
                auditModel.SetScoreLabel(scoreSystemElements);

            return Ok(ListPageResult<AuditListItemResponseModel>.Create(auditItems, count, model));
        }

        [HttpGet]
        [Route("complete/concise")]
        [ProducesResponseType(typeof(IEnumerable<IdNamePairModel<Guid>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompleteAuditConcise()
        {
            var role = CurrentUser.Role;
            if (!role.CanViewAuditsResults)
                return Forbid();

            var query = _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(a => !a.IsDeleted)
                .Where(Audit.Predicates.IsCompleted());

            var completedAuditPairs = await query
                .OrderBy(Audit.NameExpression)
                .Select(a => new IdNamePairModel<Guid> { Id = a.Id, Name = a.AuditSchedule.Name })
                .ToListAsync();

            return Ok(completedAuditPairs);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [ProducesResponseType(typeof(GetOneResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOne(Guid id)
        {
            if (!CurrentUser.Role.CanDoAudits && !CurrentUser.Role.CanViewAuditsResults)
                return Forbid();

            var currentUser = CurrentUser;
            var query = GetAuditByIdQuery(id);

            var result = await query
                .Select(GetOneResponse.GetSelectExpression())
                .FirstOrDefaultAsync();

            if (result == null)
                return Forbid();

            var template = await _context.Templates
                .AsNoTracking()
                .Include(t => t.ScoreSystem).ThenInclude(s => s.ScoreSystemElements)
                .SingleAsync(t => t.Id == result.AuditSchedule.TemplateId);

            result.Template = new TemplateInfo
            {
                Id = template.Id,
                PassedThreshold = template.ScoreSystem.PassedThreshold,
                IsAuditorSignatureRequired = template.IsAuditorSignatureRequired,
                IsAuditeeSignatureRequired = template.IsAuditeeSignatureRequired,
                SignatureAgreement = template.SignatureAgreement,
            };

            if (result.IsCompleted)
            {
                var auditItems = await _context.AuditItems
                    .AsNoTracking()
                    .Where(ai => ai.AuditId == id)
                    .Include(x => x.Children)
                    .Include(x => x.Answers)
                    .ToListAsync();

                result.AddCompletionData(template.ScoreSystem.ScoreSystemElements, auditItems);
            }
            else
            {
                var templateItems = await _context.TemplateItems
                    .AsNoTracking()
                    .Where(ti => ti.TemplateId == template.Id && !ti.IsDeleted && !ti.Template.IsDeleted)
                    .ToListAsync();

                result.Progress = new Progress
                {
                    AllItems = templateItems.Count(ti => ti.HasScoredAnswer),
                    AllSections = templateItems.Count(ti => ti.ItemType == ItemType.Section),
                };
            }

            if (result.PreviousAudits.Any())
            {
                result.PreviousAudits.First().ScoreLabel
                    = ScoreSystemElement.GetLabelByScore(template.ScoreSystem.ScoreSystemElements, result.PreviousAudits.First().Score);
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("{id:Guid}/report")]
        [ProducesResponseType(typeof(GetReportResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReport(Guid id)
        {
            if (!CurrentUser.Role.CanViewAuditsResults)
                return Forbid();

            var query = GetAuditByIdQuery(id);
            var audit = await query
                .Include(a => a.AuditItems).ThenInclude(ai => ai.Answers)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.Photos)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.AuditItemSignature).ThenInclude(s => s.CreatedBy)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.Flags)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.TemplateItem)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.CorrectiveActions).ThenInclude(ca => ca.CreatedBy)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.CorrectiveActions).ThenInclude(ca => ca.Photos)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.CorrectiveActions).ThenInclude(ca => ca.Comments)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.CorrectiveActions).ThenInclude(ca => ca.Comments).ThenInclude(com => com.CreatedBy)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.CorrectiveActions).ThenInclude(ca => ca.Comments).ThenInclude(com => com.Photos)
                .SingleAsync();

            audit.CalculateAndSetScore();

            var auditTemplateId = await _context.AuditSchedules
                .AsNoTracking()
                .Where(s => s.Id == audit.AuditScheduleId)
                .Select(s => s.TemplateId)
                .SingleAsync();

            var previousAudit = await _context.Audits
                .Where(a => !a.IsDeleted)
                .Where(a => a.IsCompleted)
                .Where(a => !audit.CompleteDate.HasValue || a.CompleteDate < audit.CompleteDate)
                .Where(a => a.AuditObjectId == audit.AuditObjectId)
                .Where(a => a.AuditSchedule.TemplateId == auditTemplateId)
                .OrderByDescending(a2 => a2.CompleteDate)
                .Include(a => a.AuditItems).ThenInclude(ai => ai.Answers)
                .FirstOrDefaultAsync();

            previousAudit?.CalculateAndSetScore();

            var scoreSystem = await _context.ScoreSystems
                .AsNoTracking()
                .Where(system => system.Templates.Any(t => t.Id== auditTemplateId))
                .Include(s => s.ScoreSystemElements)
                .SingleAsync();

            var response = new GetReportResponse(audit, scoreSystem, previousAudit);

            return Ok(response);
        }

        [HttpGet]
        [Route("{id:Guid}/report-pdf")]
        public async Task<IActionResult> GetReportPdf(Guid id, [FromQuery] AuditDetailsReportSettings settings)
        {
            if (!CurrentUser.Role.CanViewAuditsResults)
                return Forbid();

            var auditId = await GetAuditByIdQuery(id)
                .Select(a => a.Id)
                .SingleOrDefaultAsync();

            if (auditId == Guid.Empty)
                return Forbid();

            var report = await _auditDetailsReportBuilder.GetReport(id, settings);

            var reportBytes = report.GetPdfBytes();
            var fileType = report.GetFileMediaType();
            var fileName = report.GetFileName();

            return File(reportBytes, fileType, fileName);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel model)
        {
            if (!CurrentUser.Role.CanManageAudits)
                return Forbid();

            var audits = await _context.Audits
                .AsQueryable()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(a => !a.IsDeleted)
                .Where(a => model.Ids.Contains(a.Id))
                .ToListAsync();

            foreach (var audit in audits)
                audit.IsDeleted = true;

            var auditsSchedulesIds = audits.Select(a => a.AuditScheduleId).ToList();

            var oneTimeSchedules = await _context.AuditSchedules
                .AsQueryable()
                .Where(AuditSchedule.Predicates.UserHasAccess(CurrentUser))
                .Where(s => auditsSchedulesIds.Contains(s.Id))
                .Where(s => s.RepeatPattern == AuditRepeatPattern.OneTime)
                .ToListAsync();

            foreach (var oneTimeSchedule in oneTimeSchedules)
            {
                oneTimeSchedule.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            await _publisher.Publish(AuditGeneralEventMessage.CreateDeleted(CurrentUser.Id, model.Ids));

            foreach (var audit in audits)
                await ScheduleNextAudit(audit);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateAuditRequestModel model)
        {
            if (!CurrentUser.Role.CanManageAudits && !CurrentUser.Role.CanAssignAudits)
                return Forbid();

            var audit = await _context.Audits
                .AsQueryable()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(Audit.Predicates.IsNotDeleted())
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
                return Forbid();

            if (model.Assignees != null)
            {
                audit.Assignees = model.Assignees.Select(uuid => new AuditAssignee
                {
                    UserId = uuid
                }).ToList();
            }

            var isRescheduled = false;
            if (model.StartDate != null && model.EndDate != null)
            {
                if (audit.StartDate != model.StartDate
                    || audit.EndDate != model.EndDate)
                {
                    isRescheduled = true;
                }

                audit.StartDate = model.StartDate;

                audit.EndDate = model.EndDate;
                audit.IsCompleted = false;
            }

            await _context.SaveChangesAsync();

            if (model.Assignees != null)
            {
                var message = AuditGeneralEventMessage.CreateAssigned(CurrentUser.Id, id);
                await _publisher.Publish(message);
            }

            if (isRescheduled)
            {
                var message = new AuditGeneralEventMessage
                {
                    ActorUserId = CurrentUser.Id,
                    AuditIds = new List<Guid>() { audit.Id },
                    EventType = AuditGeneralEventType.Rescheduled
                };
                await _publisher.Publish(message);
            }

            return Ok();
        }

        [HttpPost]
        [Route("{id:Guid}/start")]
        public async Task<IActionResult> StartAudit(Guid id)
        {
            if (!CurrentUser.Role.CanDoAudits)
                return Forbid();

            var audit = await _context.Audits
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(Audit.Predicates.IsNotDeleted())
                .SingleOrDefaultAsync(a => a.Id == id);

            if (audit == null)
                return Forbid();

            audit.IsStarted = true;
            audit.StartedById = CurrentUser.Id;
            audit.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var message = AuditGeneralEventMessage.CreateStarted(CurrentUser.Id, id);
            await _publisher.Publish(message);

            return Ok();
        }

        [HttpPost]
        [Route("{id:Guid}/complete")]
        public async Task<IActionResult> CompleteAudit(Guid id, CompleteAuditModel model)
        {
            if (!CurrentUser.Role.CanDoAudits)
                return Forbid();

            var audit = await _context.Audits
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Include(a => a.AuditItems)
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
            {
                throw BusinessException.AuditExceptions.NotAvailable;
            }

            if (audit.IsCompleted)
            {
                throw BusinessException.AuditExceptions.Completed;
            }
            if (audit.IsDeleted)
            {
                throw BusinessException.AuditExceptions.Deleted;
            }

            model.UpdateEntity(CurrentUser, audit);
            audit.Number = audit.CalculateNumber();

            await ScheduleNextAudit(audit);

            await _context.SaveChangesAsync();

            // TODO: find out better solution to support transaction when calculating score
            // the issue is that we need ids to be generated first
            audit.CalculateAndSetScore();
            await _context.SaveChangesAsync();

            var completedMessage = AuditGeneralEventMessage.CreateCompleted(CurrentUser.Id, id);
            await _publisher.Publish(completedMessage);

            var actions = audit.AuditItems?
                .SelectMany(i => i.CorrectiveActions ?? new List<CorrectiveAction>())
                .ToList() ?? new List<CorrectiveAction>();

            foreach (var action in actions)
            {
                var assigneeIds = action.Assignees?
                    .Select(a => a.UserId)
                    .Distinct()
                    .ToList() ?? new List<Guid>();

                if (assigneeIds.Any())
                {
                    var assignedMessage = ActionGeneralEventMessage.CreateAssigned(CurrentUser.Id, action.Id, assigneeIds);
                    await _publisher.Publish(assignedMessage);
                }
            }

            return Ok();
        }

        [HttpGet]
        [Route("{id:Guid}/items")]
        [ProducesResponseType(typeof(IEnumerable<TemplateItemModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTemplateItems(Guid id)
        {
            if (!CurrentUser.Role.CanDoAudits)
                return Forbid();

            var templateId = await _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(a => a.Id == id)
                .Where(Audit.Predicates.IsNotDeleted())
                .Select(a => a.AuditSchedule.TemplateId)
                .FirstOrDefaultAsync();

            if (templateId == null || templateId.Equals(Guid.Empty))
            {
                return Forbid();
            }

            var items = await _context.TemplateItems
                .AsNoTracking()
                .Include(ti => ti.Children)
                .Where(ti => ti.TemplateId == templateId)
                .Where(ti => !ti.IsDeleted)
                .Select(ti => new TemplateItemModel
                {
                    Id = ti.Id,
                    ParentId = ti.ParentId,
                    ChildrenIds = ti.Children
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.Index)
                        .Select(c => c.Id),
                    Index = ti.Index,
                    GroupIndex = ti.ItemType == ItemType.Item
                        && ti.ParentId.HasValue
                        && ti.Parent.ParentId.HasValue
                        && ti.Parent.ItemType == ItemType.Condition
                        && ti.Parent.Parent.ItemType == ItemType.ConditionalItem
                        ? ti.Parent.Parent.Index
                        : (int?)null,
                    ItemType = ti.ItemType,
                    AnswerType = ti.AnswerType,
                    Text = ti.Text,
                    Data = ti.Answers
                        .OrderBy(a => a.Index)
                        .Select(a => a),
                    ConditionValue = ti.ConditionValue,
                    TagsIds = ti.Tags.Select(t => t.TagId),
                    IsPhotoRequired = ti.IsPhotoRequired,
                    IsSignatureRequired = ti.IsSignatureRequired,
                    HasInformation = ti.HasInformation,
                    Information = new ItemInformation
                    {
                        Text = ti.InformationText,
                        PhotosIds = ti.InformationPhotos.Select(p => p.Id)
                    }
                })
                .ToListAsync();

            var auditObjectTags = await _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(a => a.Id == id)
                .SelectMany(a => a.AuditObject.AuditObjectTags.Select(aot => aot.TagId))
                .ToListAsync();

            items = TemplateItemModel.FilterItemsByTags(items, auditObjectTags);
            var root = items.Single(i => i.ItemType == ItemType.Root);
            root.InitTree(items);
            items = root.GetItems();

            return Ok(items);
        }

        private IQueryable<Audit> GetAuditByIdQuery(Guid auditId)
        {
            var query = _context.Audits
                .AsNoTracking()
                .Where(Audit.Predicates.UserHasAccess(CurrentUser))
                .Where(a => !a.IsDeleted)
                .Where(audit => audit.Id == auditId);

            if (!CurrentUser.Role.CanDoAudits)
                query = query.Where(Audit.Predicates.AuditStatuses.IsCompleted);

            return query;
        }

        private async Task ScheduleNextAudit(Audit previousAudit)
        {
            var schedule = await _context.AuditSchedules
                .AsQueryable()
                .Where(s => !s.IsDeleted)
                .Where(s => s.Id == previousAudit.AuditScheduleId)
                .FirstOrDefaultAsync();

            if (schedule == null)
                return;

            var domainModel = new AuditDomainModel(previousAudit);

            var nextAudit = await domainModel.GetNextAudit(schedule, _context);

            if (nextAudit == null)
            {
                schedule.IsDeleted = true;
                return;
            }

            _context.Audits.Add(nextAudit);
        }
    }
}
