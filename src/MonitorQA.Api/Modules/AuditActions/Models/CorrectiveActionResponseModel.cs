using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.AuditTemplates.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditActions.Models
{
    public class CorrectiveActionResponseModel
    {
        public Guid Id { get; set; }

        public Guid? AuditItemId { get; set; }

        public Guid TemplateItemId { get; set; }

        public string Name { get; set; }

        public DateTime? DueDate { get; set; }

        public CorrectiveActionStatus Status { get; set; }

        public CorrectiveActionPriority Priority { get; set; }

        public string Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public IdNamePairModel<Guid>? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public AuditResponseModel Audit { get; set; }

        public IdNamePairModel<Guid> CreatedBy { get; set; }

        public IEnumerable<IdNamePairModel<Guid>> Assignees { get; set; }

        public IEnumerable<PhotoModel> Photos { get; set; }

        public IEnumerable<CommentResponseModel> Comments { get; set; }

        public bool CanApprove { get; set; }

        public bool CanAssign { get; set; }

        public bool CanMarkAsDone { get; set; }

        public ItemInformation? Information { get; set; }

        public class AuditResponseModel
        {
            public Guid Id { get; set; }
            
            public Guid AuditId { get; set; }

            public string Name { get; set; }

            public DateTime? StartDate { get; set; }
        }

        public class CommentResponseModel
        {
            public Guid Id { get; set; }

            public string Body { get; set; }

            public DateTime CreatedAt { get; set; }

            public IdNamePairModel<Guid> CreatedBy { get; set; }

            public IEnumerable<PhotoResponseModel> Photos { get; set; }
        }

        public class PhotoResponseModel
        {
            public Guid Id { get; set; }
        }

        

        public static Expression<Func<CorrectiveAction, CorrectiveActionResponseModel>> GetSelectExpression(User CurrentUser)
        {
            return a => new CorrectiveActionResponseModel
            {
                Id = a.Id,
                AuditItemId = a.AuditItemId,
                TemplateItemId = a.AuditItem.TemplateItemId,
                Name = a.Name,
                DueDate = a.DueDate,
                Status = a.Status,
                Priority = a.Priority,
                Description = a.Description,
                Audit = new AuditResponseModel
                {
                    AuditId = a.AuditItem.AuditId,
                    Id = a.AuditItem.AuditId,
                    Name = a.AuditItem.Audit.AuditSchedule.Name,
                    StartDate = a.AuditItem.Audit.StartDate
                },
                CreatedBy = new IdNamePairModel<Guid>
                {
                    Id = a.CreatedBy.Id,
                    Name = a.CreatedBy.Name
                },
                CreatedAt = a.CreatedAt,
                Assignees = a.Assignees.Select(aa => new IdNamePairModel<Guid>
                {
                    Id = aa.User.Id,
                    Name = aa.User.Name
                }),
                ApprovedAt = a.ApprovedAt,
                ApprovedBy = new IdNamePairModel<Guid>
                {
                    Id = a.ApprovedBy.Id,
                    Name = a.ApprovedBy.Name
                },
                CanApprove = CurrentUser.Role.CanApproveCorrectiveActions,
                CanAssign = CurrentUser.Role.CanAssignCorrectiveActions,
                CanMarkAsDone = a.Status != CorrectiveActionStatus.Approved &&
                                a.Assignees.Any(aa => aa.UserId == CurrentUser.Id),
                Photos = a.Photos.Select(p => new PhotoModel
                {
                    Id = p.Id,
                    Note = p.Note,
                    UpdatedAt = p.UpdatedAt
                }),
                Comments = a.Comments.Select(c => new CommentResponseModel
                {
                    Id = c.Id,
                    Body = c.Body,
                    CreatedBy = new IdNamePairModel<Guid>
                    {
                        Id = c.CreatedBy.Id,
                        Name = c.CreatedBy.Name
                    },
                    CreatedAt = c.CreatedAt,
                    Photos = c.Photos.Select(p => new PhotoResponseModel
                    {
                        Id = p.Id
                    })
                }).ToList()
            };
        }
    }
}
