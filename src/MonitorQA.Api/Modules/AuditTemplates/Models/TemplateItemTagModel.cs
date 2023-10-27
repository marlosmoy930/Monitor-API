using System;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models
{
    public class TemplateItemTagModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public TemplateItemTagModel(TemplateItemTag entity)
        {
            Id = entity.Tag.Id;
            Name = entity.Tag.Name;
            Color = entity.Tag.Color;
        }
    }
}