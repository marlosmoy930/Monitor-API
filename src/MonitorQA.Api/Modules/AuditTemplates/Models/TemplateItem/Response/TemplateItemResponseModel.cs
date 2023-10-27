using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.TemplateItem.Response
{
    public class TemplateItemResponseModel
    {
        public Guid Id { get; }

        public bool IsDeleted { get; set; }

        public Guid TemplateId { get; set; }

        public AnswerType? AnswerType { get; }

        public Guid? ParentId { get; set; }

        public List<string> ChildrenIds { get; }

        public int Index { get; set; }

        public ItemType ItemType { get; }

        public string Text { get; }

        public Condition? Condition { get; set; }

        public bool IsSignatureRequired { get; set; }

        public bool IsPhotoRequired { get; set; }

        public bool HasInformation { get; set; }

        public ItemInformation? Information { get; set; }

        public List<object>? Data { get; }

        public List<TemplateItemTagModel> Tags { get; set; }

        public TemplateItemResponseModel(Data.Entities.TemplateItem entity, List<Data.Entities.TemplateItem> templateItems)
        {
            Id = entity.Id;
            IsDeleted = entity.IsDeleted;
            TemplateId = entity.TemplateId;
            AnswerType = entity.AnswerType;
            ParentId = entity.ParentId;
            ChildrenIds = templateItems
                .Where(x => x.ParentId == entity.Id)
                .Select(c => c.Id.ToString())
                .ToList();
            Index = entity.Index;
            ItemType = entity.ItemType;
            Text = entity.Text;
            IsSignatureRequired = entity.IsSignatureRequired;
            IsPhotoRequired = entity.IsPhotoRequired;
            HasInformation = entity.HasInformation;
            Information = new ItemInformation
            {
                Text = entity.InformationText,
                PhotosIds = entity.InformationPhotos.Select(p => p.Id)
            };

            if (AnswerType.HasValue)
            {
                Data = entity.Answers
                    .Select(ans => (object) AnswerDataBase.Create(AnswerType.Value, ans))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(entity.ConditionValue))
            {
                Condition = Condition.Create(entity);
            }

            Tags = entity.Tags.Select(t => new TemplateItemTagModel(t)).ToList();
        }
    }
}