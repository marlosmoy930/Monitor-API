using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.TemplateItem.Request
{
    public class TemplateItemRequestModel
    {
        public string? Text { get; set; }

        public ItemType? ItemType { get; set; }

        public AnswerType? AnswerType { get; set; }

        public Guid? ParentId { get; set; }

        public int? Index { get; set; }

        public Condition? Condition { get; set; }

        public bool? IsSignatureRequired { get; set; }

        public bool? IsPhotoRequired { get; set; }

        public bool? HasInformation { get; set; }

        public ItemInformation? Information { get; set; }

        public List<Guid>? TagsIds { get; set; }

        public object? Data { get; set; }

        public void UpdateEntity(Data.Entities.TemplateItem item, List<Data.Entities.TemplateItem> templateItems)
        {
            Condition?.UpdateEntity(item);

            if (ItemType.HasValue)
                item.ItemType = ItemType.Value;

            if (AnswerType.HasValue)
                item.AnswerType = AnswerType.Value;

            if (Text != null)
                item.Text = Text;

            if (IsSignatureRequired.HasValue)
                item.IsSignatureRequired = IsSignatureRequired.Value;

            if (IsPhotoRequired.HasValue)
                item.IsPhotoRequired = IsPhotoRequired.Value;

            if (HasInformation.HasValue) 
                item.HasInformation = HasInformation.Value;

            if (Information != null)
            {
                item.InformationText = Information.Text;
                item.InformationPhotos = Information.PhotosIds.Select(uuid => new TemplateItemInformationPhoto
                {
                    Id = uuid,
                    TemplateItemId = item.Id
                }).ToList();
            }

            if (TagsIds != null)
            {
                item.Tags = TagsIds.Select(uuid => new TemplateItemTag
                {
                    TagId = uuid,
                }).ToList();
            }

            UpdateIndexes(item, templateItems);

            UpdateAnswers(item);
        }

        private void UpdateIndexes(Data.Entities.TemplateItem item, List<Data.Entities.TemplateItem> templateItems)
        {
            if (!Index.HasValue) return;

            var targetParent = ParentId.HasValue
                ? templateItems.Single(i => i.Id == ParentId)
                : templateItems.Single(i => i.Id == item.ParentId);

            MoveItem(item, targetParent, Index);

            if (item.ParentId != targetParent.Id)
            {
                MoveItem(item, targetParent, null);
                item.ParentId = ParentId;
            }
        }

        private static void MoveItem(Data.Entities.TemplateItem item, Data.Entities.TemplateItem parent, int? index)
        {
            var siblings = parent
                .Children
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.Index)
                .ToList();

            if (index.HasValue)
            {
                siblings.Insert(index.Value, item);
            }

            for (int i = 0; i < siblings.Count; i++)
            {
                siblings[i].Index = i;
            }
        }

        private void UpdateAnswers(Data.Entities.TemplateItem item)
        {
            if (item.AnswerType == null || Data == null)
                return;

            var answers = AnswerDataBase.Create(item.AnswerType.Value, Data);
            UpdateTemplateItemEntityAnswers(item, answers);
        }

        private static void UpdateTemplateItemEntityAnswers(Data.Entities.TemplateItem item, List<AnswerDataBase> source)
        {
            item.Answers = new List<TemplateItemAnswer>();

            for (var i = 0; i < source.Count; i++)
            {
                var checklistData = source[i];
                item.Answers.Add(checklistData.GetTemplateItemAnswerEntity(item, i));
            }
        }
    }
}