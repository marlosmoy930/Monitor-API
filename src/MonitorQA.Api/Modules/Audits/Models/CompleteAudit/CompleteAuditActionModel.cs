using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class CompleteAuditActionModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public IdModel CreatedBy { get; set; }

        public DateTime? DueDate { get; set; }

        public CorrectiveActionPriority Priority { get; set; }

        public CorrectiveActionStatus Status { get; set; }

        public IEnumerable<CorrectiveActionCommentModel>? Comments { get; set; }

        public IEnumerable<PhotoModel> Photos { get; set; } = new List<PhotoModel>();

        public IEnumerable<Guid> Assignees { get; set; }

        public CorrectiveAction GetCorrectiveAction()
        {
            return new CorrectiveAction
            {
                Id = Id,
                Name = Name,
                Description = Description,
                CreatedAt = CreatedAt,
                DueDate = DueDate,
                Priority = Priority,
                Status = Status,
                CreatedById = CreatedBy.Id,
                Comments = Comments.Select(c => new CorrectiveActionComment
                {
                    Id = c.Id,
                    Body = c.Body,
                    CreatedById = c.CreatedBy.Id,
                    CreatedAt = c.CreatedAt,
                    Photos = c.Photos.Select(i => new CorrectiveActionCommentPhoto
                    {
                        Id = i.Id,
                        Note = i.Note,
                        UpdatedAt = i.UpdatedAt
                    }).ToList()
                }).ToList(),
                Assignees = Assignees.Select(userId => new CorrectiveActionAssignee
                {
                    CorrectiveActionId = Id,
                    UserId = userId,
                }).ToList(),
                Photos = Photos?.Select(a => new CorrectiveActionPhoto
                {
                    Id = a.Id,
                    Note = a.Note,
                    UpdatedAt = a.UpdatedAt
                }).ToList()
            };
        }
    }
}