using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditActions.Models
{
    public class ActionCommentModel
    {
        public Guid Id { get; set; }

        public string Body { get; set; }

        public IEnumerable<PhotoModel> Photos { get; set; } = new List<PhotoModel>();


        public CorrectiveActionComment CreateCorrectiveActionComment(User sender, Guid actionId)
        {
            return new CorrectiveActionComment
            {
                Id = Id,
                ActionId = actionId,
                CreatedById = sender.Id,
                CreatedAt = DateTime.UtcNow,
                Body = Body,
                Photos = Photos.Select(i => new CorrectiveActionCommentPhoto
                {
                    Id = i.Id,
                    Note = i.Note,
                    CommentId = Id,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            };
        }

        public void UpdateEntity(CorrectiveActionComment entity)
        {
            entity.Body = Body;
            entity.Photos = Photos.Select(i => new CorrectiveActionCommentPhoto
            {
                Id = i.Id,
                Note = i.Note,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
        }
    }
}