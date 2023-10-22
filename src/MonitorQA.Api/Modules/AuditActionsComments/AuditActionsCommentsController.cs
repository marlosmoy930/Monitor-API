using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Modules.AuditActions.Models;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Firebase;
using MonitorQA.Notifications.CorrectiveActions;
using MonitorQA.Utils.Configurations;

namespace MonitorQA.Api.Modules.AuditActionsComments
{
    [Route("audit/actions/{actionId:Guid}/comment")]
    [ApiController]
    public class AuditActionsCommentsController : AuthorizedController
    {
        private readonly SiteContext _context;
        private readonly CloudMessagePublisher _publisher;
        private readonly ConfigurationData _configurationData;
        private readonly StorageClient _storageClient;

        public AuditActionsCommentsController(
            SiteContext context,
            CloudMessagePublisher publisher,
            ConfigurationData configurationData,
            StorageClient storageClient) : base(context)
        {
            _context = context;
            this._publisher = publisher;
            this._configurationData = configurationData;
            this._storageClient = storageClient;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid actionId, ActionCommentModel model)
        {
            var hasAccessQuery = _context.CorrectiveActions
                .AsNoTracking()
                .Where(CorrectiveAction.Predicates.UserHasAccess(CurrentUser))
                .Where(CorrectiveAction.Predicates.IsNotDeleted())
                .Where(a => a.Id == actionId);

            var hasAccess = await hasAccessQuery.AnyAsync();
            if (!hasAccess)
                return Forbid();

            var entity = model.CreateCorrectiveActionComment(CurrentUser, actionId);
            _context.CorrectiveActionsComments.Add(entity);
            await _context.SaveChangesAsync();

            var request = new ActionGeneralEventMessage
            {
                ActorUserId = CurrentUser.Id,
                ActionIds = new List<Guid>() { actionId },
                EventType = ActionGeneralEventType.CommentAdded
            };
            await _publisher.Publish(request);

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateComment(ActionCommentModel model)
        {
            var comment = await _context.CorrectiveActionsComments
                .Where(CorrectiveActionComment.Predicates.UserHasAccess(CurrentUser))
                .Include(c => c.Photos)
                .SingleAsync(c => c.Id == model.Id);

            if (comment.CreatedById != CurrentUser.Id)
                return Forbid();

            var hasNextComments = await GetHasNextComments(comment);
            if (hasNextComments)
                return Forbid();

            model.UpdateEntity(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var comment = await _context.CorrectiveActionsComments
                .Where(CorrectiveActionComment.Predicates.UserHasAccess(CurrentUser))
                .Include(c => c.Photos)
                .SingleAsync(c => c.Id == commentId);

            if (comment.CreatedById != CurrentUser.Id)
                return Forbid();

            var hasNextComments = await GetHasNextComments(comment);
            if (hasNextComments)
                return Forbid();

            var tasks = new List<Task>();

            if (comment.Photos.Any())
            {
                var photoInfoItems = await _context.CorrectiveActionCommentPhoto
                    .AsNoTracking()
                    .Where(photo => photo.CommentId == commentId)
                    .Select(photo => new StorageActionCommentPhotoInfo
                    {
                        CompanyId = photo.Comment.Action.AuditItem.Audit.AuditObject.CompanyId,
                        AuditId = photo.Comment.Action.AuditItem.AuditId,
                        TemplateitemId = photo.Comment.Action.AuditItem.TemplateItemId,
                        ActionId = photo.Comment.ActionId,
                        CommentId = photo.CommentId,
                        PhotoId = photo.Id,
                    })
                    .ToListAsync();

                var bucket = _configurationData.Firebase.GetBucket();

                var deletePhotoTasks = photoInfoItems
                    .Select(item =>
                    {
                        var photoObjectName = item.GetStorageObjectName();
                        return _storageClient.DeleteObjectAsync(bucket, photoObjectName);
                    })
                    .ToList();

                tasks.AddRange(deletePhotoTasks);
            }

            _context.CorrectiveActionsComments.Remove(comment);
            tasks.Add(_context.SaveChangesAsync());

            await Task.WhenAll(tasks);

            return Ok();
        }

        private async Task<bool> GetHasNextComments(CorrectiveActionComment comment)
        {
            return await _context.CorrectiveActionsComments
                            .AsNoTracking()
                            .Where(CorrectiveActionComment.Predicates.UserHasAccess(CurrentUser))
                            .Where(c => c.ActionId == comment.ActionId)
                            .Where(c => c.CreatedAt > c.CreatedAt)
                            .AnyAsync();
        }
    }
}
