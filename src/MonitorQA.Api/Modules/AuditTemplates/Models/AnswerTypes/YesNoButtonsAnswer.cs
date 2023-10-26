using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes
{
    public class YesNoButtonsAnswer : AnswerDataBase
    {
        public int? Points { get; set; }

        public bool MarkAsFailed { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public static YesNoButtonsAnswer Create(TemplateItemAnswer answer)
        {
            return new YesNoButtonsAnswer
            {
                Points = answer.Points,
                MarkAsFailed = answer.MarkAsFailed,
                Color = answer.Color,
                Name = answer.Name,
            };
        }

        public static YesNoButtonsAnswer[] CreateYesNo()
        {
            var passed = new YesNoButtonsAnswer
            {
                Name = TemplateItemAnswer.Yes.Name,
                Color = TemplateItemAnswer.Yes.Color,
                Points = TemplateItemAnswer.Yes.Points,
                MarkAsFailed = false
            };

            var failed = new YesNoButtonsAnswer
            {
                Name = TemplateItemAnswer.No.Name,
                Color = TemplateItemAnswer.No.Color,
                Points = TemplateItemAnswer.No.Points,
                MarkAsFailed = true
            };

            return new[] { passed, failed };
        }

        public override TemplateItemAnswer GetTemplateItemAnswerEntity(Data.Entities.TemplateItem item, int index)
        {
            return new TemplateItemAnswer
            {
                Index = index,
                TemplateItemId = item.Id,
                TemplateItem = item,
                Points = Points,
                MarkAsFailed = MarkAsFailed,
                Color = Color,
                Name = Name
            };
        }
    }
}