using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditActions.Models
{
    public class CorrectiveActionModel
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public CorrectiveActionPriority? Priority { get; set; }

        public CorrectiveActionStatus? Status { get; set; }

        public List<Guid>? Assignees { get; set; }

        public IEnumerable<PhotoModel>? Photos { get; set; }

        public List<Guid> UpdateEntityAndGetAssigneesToNotify(Guid actorUserId, CorrectiveAction entity)
        {
            List<Guid> assigneesToNotify = new List<Guid>();

            if (Name != null)
                entity.Name = Name;

            if (Description != null)
                entity.Description = Description;

            if (Status.HasValue)
                entity.Status = Status.Value;

            if (DueDate.HasValue)
                entity.DueDate = DueDate;

            if (Priority.HasValue)
                entity.Priority = Priority.Value;

            if (Assignees != null)
            {
                var assignees = Assignees.Select(uuid => new CorrectiveActionAssignee
                {
                    CorrectiveActionId = entity.Id,
                    UserId = uuid
                }).ToList();

                var existingAssigneeIds = entity.Assignees.Select(a => a.UserId);
                assigneesToNotify = assignees
                    .Where(a => a.UserId != actorUserId)
                    .Where(a => !existingAssigneeIds.Contains(a.UserId))
                    .Select(a => a.UserId)
                    .ToList();

                entity.Assignees = assignees;
            }

            if (Photos != null)
                entity.Photos = Photos.Select(p => new CorrectiveActionPhoto
                {
                    Id = p.Id,
                    Note = p.Note,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

            return assigneesToNotify;
        }
    }
}