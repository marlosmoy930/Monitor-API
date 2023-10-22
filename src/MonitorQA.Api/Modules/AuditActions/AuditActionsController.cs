using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.AuditActions.Models;
using MonitorQA.Api.Modules.AuditActions.Models.ListModels;
using MonitorQA.Api.Modules.AuditTemplates.Models;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Notifications.CorrectiveActions;

namespace MonitorQA.Api.Modules.AuditActions
{
    [Route("audit/actions")]
    [ApiController]
    public class AuditActionsController : AuthorizedController
    {
        private readonly SiteContext _context;
        private readonly CloudMessagePublisher _publisher;

        public AuditActionsController(
            SiteContext context,
            CloudMessagePublisher publisher)
            : base(context)
        {
            _context = context;
            this._publisher = publisher;
        }

        [HttpGet]
        [Route("detailed")]
        [ProducesResponseType(typeof(ListPageResult<AuditActionListItemResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList([FromQuery] ActionsListRequestModel model)
        {
            var result = await GetFilteredAndOrderedList(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("unresolved")]
        [ProducesResponseType(typeof(ListPageResult<AuditActionListItemResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnresolvedCorrectiveActions([FromQuery] ActionsListRequestModel model)
        {
            model.Statuses = new List<CorrectiveActionStatus>() {
                 CorrectiveActionStatus.Open, CorrectiveActionStatus.Rejected, CorrectiveActionStatus.Submitted
            };

            if (string.IsNullOrEmpty(model.OrderBy))
                model.OrderBy = ActionsListRequestModel.OrderByDueDateFieldName;

            if (string.IsNullOrEmpty(model.OrderByDirection))
                model.OrderByDirection = ListRequestModel.AscendingOrderDirection;

            var result = await GetFilteredAndOrderedList(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("resolved")]
        [ProducesResponseType(typeof(ListPageResult<AuditActionListItemResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResolvedCorrectiveActions([FromQuery] ActionsListRequestModel model)
        {
            model.Statuses = new List<CorrectiveActionStatus>() { CorrectiveActionStatus.Approved };

            if (string.IsNullOrEmpty(model.OrderBy))
                model.OrderBy = ActionsListRequestModel.OrderByApprovalDateFieldName;

            if (string.IsNullOrEmpty(model.OrderByDirection))
                model.OrderByDirection = ListRequestModel.DescendingOrderDirection;

            var result = await GetFilteredAndOrderedList(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("approve")]
        public async Task<IActionResult> Approve(IdsArrayModel<Guid> model)
        {
            if (!CurrentUser.Role.CanDoAudits)
                return Forbid();

            var entities = await _context.CorrectiveActions
                .AsQueryable()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(a => model.Ids.Contains(a.Id))
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.Status = CorrectiveActionStatus.Approved;
                entity.ApprovedById = CurrentUser.Id;
                entity.ApprovedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var request = new ActionGeneralEventMessage
            {
                ActorUserId = CurrentUser.Id,
                ActionIds = model.Ids,
                EventType = ActionGeneralEventType.Approved
            };
            await _publisher.Publish(request);

            return Ok();
        }

        [HttpPost]
        [Route("reject")]
        public async Task<IActionResult> Reject(IdsArrayModel<Guid> model)
        {
            if (!CurrentUser.Role.CanDoAudits)
                return Forbid();

            await SetStatus(model, CorrectiveActionStatus.Rejected);

            var request = new ActionGeneralEventMessage
            {
                ActorUserId = CurrentUser.Id,
                ActionIds = model.Ids,
                EventType = ActionGeneralEventType.Rejected
            };
            await _publisher.Publish(request);
            return Ok();
        }

        [HttpPost]
        [Route("submit")]
        public async Task<IActionResult> Submit(IdsArrayModel<Guid> model)
        {
            var entities = await _context.CorrectiveActions
                .AsQueryable()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(a => model.Ids.Contains(a.Id))
                .ToListAsync();

            foreach (var entity in entities)
                entity.Status = CorrectiveActionStatus.Submitted;

            await _context.SaveChangesAsync();

            var request = new ActionGeneralEventMessage
            {
                ActorUserId = CurrentUser.Id,
                ActionIds = model.Ids,
                EventType = ActionGeneralEventType.Submitted
            };
            await _publisher.Publish(request);

            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = _context.CorrectiveActions
                .AsNoTracking()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(a => a.Id == id);

            var responseModel = await query
            .Select(CorrectiveActionResponseModel.GetSelectExpression(CurrentUser)).FirstOrDefaultAsync();

            if (responseModel == null)
                return Forbid();

            var templateItem = await _context
                .TemplateItems
                .Where(ti => ti.Id == responseModel.TemplateItemId)
                .Include(ti => ti.InformationPhotos)
                .FirstOrDefaultAsync();

            if (templateItem != null && templateItem.HasInformation)
            {
                responseModel.Information = new ItemInformation
                {
                    Text = templateItem.InformationText,
                    PhotosIds = templateItem.InformationPhotos.Select(p => p.Id)
                };
            }

            return Ok(responseModel);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CorrectiveActionModel model)
        {
            var entity = await _context.CorrectiveActions
                .AsQueryable()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(ca => ca.Id == id)
                .Include(a => a.Assignees)
                .Include(a => a.Photos)
                .FirstOrDefaultAsync();

            if (entity == null)
                return Forbid();

            var assigneesToNotify = model.UpdateEntityAndGetAssigneesToNotify(CurrentUser.Id, entity);

            await _context.SaveChangesAsync();

            if (assigneesToNotify.Any())
            {
                var message = ActionGeneralEventMessage.CreateAssigned(CurrentUser.Id, id, assigneesToNotify);
                await _publisher.Publish(message);
            }

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel<Guid> model)
        {
            var ids = model.Ids.ToList();
            var entities = await _context.CorrectiveActions
                .AsQueryable()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(ca => ids.Contains(ca.Id))
                .ToListAsync();

            foreach (var correctiveAction in entities)
                correctiveAction.IsDeleted = true;

            await _context.SaveChangesAsync();

            var request = new ActionGeneralEventMessage
            {
                ActorUserId = CurrentUser.Id,
                ActionIds = ids,
                EventType = ActionGeneralEventType.Deleted
            };
            await _publisher.Publish(request);

            return Ok();
        }

        private async Task SetStatus(IdsArrayModel<Guid> model, CorrectiveActionStatus status)
        {
            var entities = await _context.CorrectiveActions
                .AsQueryable()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(a => model.Ids.Contains(a.Id))
                .ToListAsync();

            foreach (var entity in entities)
                entity.Status = status;

            await _context.SaveChangesAsync();
        }

        private async Task<ListPageResult<AuditActionListItemResponseModel>> GetFilteredAndOrderedList(ActionsListRequestModel model)
        {
            var query = _context.CorrectiveActions
                .AsNoTracking()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted());

            query = model.ApplyFilter(query, CurrentUser);

            var count = await query.CountAsync();

            query = model.ApplyOrder(query);

            var data = await query
                .Select(AuditActionListItemResponseModel.GetSelectExpression(CurrentUser))
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .ToListAsync();

            var result = ListPageResult<AuditActionListItemResponseModel>.Create(data, count, model);
            return result;
        }
    }
}