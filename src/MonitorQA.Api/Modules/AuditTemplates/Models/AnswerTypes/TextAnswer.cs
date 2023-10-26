using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes
{
    public class TextAnswer : AnswerDataBase
    {
        public string Text { get; set; }

        public static TextAnswer Create(TemplateItemAnswer answer)
        {
            return new TextAnswer
            {
                Text = answer.Text
            };
        }

        public override TemplateItemAnswer GetTemplateItemAnswerEntity(Data.Entities.TemplateItem item, int index)
        {
            return new TemplateItemAnswer
            {
                Index = index,
                TemplateItemId = item.Id,
                TemplateItem = item,
                Text = Text,
            };
        }
    }
}