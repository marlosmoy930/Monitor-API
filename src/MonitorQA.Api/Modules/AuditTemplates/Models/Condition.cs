using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models
{
    public class Condition
    {
        public string? Value { get; set; }

        public string? Field { get; set; }

        public string? Compare { get; set; }

        internal void UpdateEntity(Data.Entities.TemplateItem item)
        {
            item.ConditionValue = Value;
            item.ConditionField = Field;
            item.ConditionCompare = Compare;
        }

        internal static Condition? Create(Data.Entities.TemplateItem entity)
        {
            var condition = new Condition {
                Value = entity.ConditionValue,
                Field = entity.ConditionField,
                Compare = entity.ConditionCompare,
            };
            return condition;
        }
    }
}