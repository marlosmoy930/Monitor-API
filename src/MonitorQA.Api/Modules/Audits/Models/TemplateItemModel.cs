using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Api.Modules.AuditTemplates.Models;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Audits.Models
{
    public class TemplateItemModel
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public IEnumerable<Guid> ChildrenIds { get; set; }

        public int Index { get; set; }

        public int? GroupIndex { get; set; }

        public ItemType ItemType { get; set; }

        public AnswerType? AnswerType { get; set; }

        public string Text { get; set; }

        public string ConditionValue { get; set; }

        public bool IsPhotoRequired { get; set; }

        public bool IsSignatureRequired { get; set; }

        public bool HasInformation { get; set; }

        public ItemInformation? Information { get; set; }

        public IEnumerable<TemplateItemAnswer> Data { get; set; }

        public IEnumerable<Guid> TagsIds { get; set; }

        private readonly List<TemplateItemModel> _items = new List<TemplateItemModel>();

        public void InitTree(List<TemplateItemModel> allAuditItems)
        {
            var children = allAuditItems.Where(x => x.ParentId == Id).ToList();

            foreach (var child in children)
            {
                _items.Add(child);
                child.InitTree(allAuditItems);
            }
        }

        public List<TemplateItemModel> GetItems()
        {
            var result = new List<TemplateItemModel>();

            if (ItemType == ItemType.Root)
            {
                result.Add(this);
            }

            result.AddRange(_items);

            foreach (var item in _items)
            {
                result.AddRange(item.GetItems());
            }

            return result;
        }

        public static List<TemplateItemModel> FilterItemsByTags(
            IReadOnlyCollection<TemplateItemModel> items,
            IReadOnlyCollection<Guid> auditObjectTagsIds)
        {
            var result = new List<TemplateItemModel>();
            foreach (var itemModel in items)
            {
                if (itemModel.TagsIds == null || !itemModel.TagsIds.Any())
                {
                    result.Add(itemModel);
                    continue;
                }

                if (itemModel.TagsIds.All(auditObjectTagsIds.Contains!))
                {
                    result.Add(itemModel);
                }
            }

            return result;
        }
    }
}
