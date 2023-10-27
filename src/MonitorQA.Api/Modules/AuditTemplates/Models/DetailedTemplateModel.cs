using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MonitorQA.Api.Modules.AuditTemplates.Models
{
    public class DetailedTemplateModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public bool IsDraft { get; set; }

        public TemplateType TemplateType { get; set; }

        public bool? IsAuditorSignatureRequired { get; set; }

        public bool? IsAuditeeSignatureRequired { get; set; }

        public IdNamePairModel<Guid> ScoreSystem { get; set; }

        public IEnumerable<TagInfo> Tags { get; set; }

        public static Expression<Func<Template, DetailedTemplateModel>> GetExpression()
        {
            return t => new DetailedTemplateModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsDraft = t.IsDraft,
                TemplateType = t.TemplateType,
                IsAuditorSignatureRequired = t.IsAuditorSignatureRequired,
                IsAuditeeSignatureRequired = t.IsAuditeeSignatureRequired,
                ScoreSystem = new IdNamePairModel<Guid>
                {
                    Id = t.ScoreSystem.Id,
                    Name = t.ScoreSystem.Name
                },
                Tags = t.TemplateTags
                        .OrderBy(tags => tags.Tag.Name)
                        .Select(tags => new TagInfo
                        {
                            Id = tags.Tag.Id,
                            Name = tags.Tag.Name,
                            Color = tags.Tag.Color
                        })
            };
        }
    }
}