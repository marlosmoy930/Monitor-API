using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.AuditObjects.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditActions.Models.ListModels
{
    public class AuditActionListItemResponseModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid TemplateItemId { get; set; }

        public IdNamePairModel<Guid> CreatedBy { get; set; }

        public IEnumerable<IdNamePairModel<Guid>> Assignees { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string Description { get; set; }

        public CorrectiveActionStatus Status { get; set; }

        public CorrectiveActionPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public AuditModel Audit { get; set; }

        public AuditObjectModel AuditObject { get; set; }

        public DateTime LastUpdatedBy { get; set; }

        public DateTime? CompletedAt { get; set; }

        public IdNamePairModel<Guid> ApprovedBy { get; set; }

        public bool CanApprove { get; set; }

        public bool CanAssign { get; set; }

        public bool CanMarkAsDone { get; set; }
        
        public IEnumerable<PhotoModel> Photos { get; set; }

        public static Expression<Func<CorrectiveAction, AuditActionListItemResponseModel>> GetSelectExpression(User user)
        {
            return action => new AuditActionListItemResponseModel
            {
                Id = action.Id,
                Name = action.Name,
                TemplateItemId = action.AuditItem.TemplateItemId,
                CreatedBy = new IdNamePairModel<Guid>
                {
                    Id = action.CreatedBy.Id,
                    Name = action.CreatedBy.Name
                },
                Assignees = action.Assignees.Select(aa => new IdNamePairModel<Guid>
                {
                    Id = aa.User.Id,
                    Name = aa.User.Name
                }),
                CreatedAt = action.CreatedAt,
                Description = action.Description,
                Status = action.Status,
                Priority = action.Priority,
                DueDate = action.DueDate,
                Audit = new AuditModel
                {
                    Id = action.AuditItem.AuditId,
                    Name = action.AuditItem.Audit.AuditSchedule.Name,
                    Number = action.AuditItem.Audit.Number
                },
                AuditObject = new AuditObjectModel
                {
                    Id = action.AuditItem.Audit.AuditObject.Id,
                    Name = action.AuditItem.Audit.AuditObject.Name,
                    Address = new GeoAddress
                    {
                        Address = action.AuditItem.Audit.AuditObject.Address,
                        Name = action.AuditItem.Audit.AuditObject.AddressName,
                        Lat = action.AuditItem.Audit.AuditObject.Latitude,
                        Lng = action.AuditItem.Audit.AuditObject.Longitude
                    }
                },
                LastUpdatedBy = action.Notifications
                        .OrderByDescending(n => n.CreatedAt)
                        .DefaultIfEmpty()
                        .Select(n => n.CreatedAt)
                        .FirstOrDefault(),
                CompletedAt = action.ApprovedAt,
                ApprovedBy = new IdNamePairModel<Guid>
                {
                    Id = action.ApprovedBy.Id,
                    Name = action.ApprovedBy.Name
                },
                CanApprove = user.Role.CanApproveCorrectiveActions,
                CanAssign = user.Role.CanAssignCorrectiveActions,
                CanMarkAsDone = action.Assignees.Any(aa => aa.UserId == user.Id),
                Photos = action.Photos.Select(p => new PhotoModel
                {
                    Id = p.Id,
                    Note = p.Note,
                    UpdatedAt = p.UpdatedAt,
                })
            };
        }

    }
}