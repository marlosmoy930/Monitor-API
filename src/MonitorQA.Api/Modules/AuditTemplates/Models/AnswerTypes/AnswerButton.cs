using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes
{
    public class AnswerButton : AnswerDataBase
    {
        public int? Points { get; set; }

        public bool MarkAsFailed { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public static AnswerButton Create(TemplateItemAnswer answer)
        {
            return new AnswerButton
            {
                Points = answer.Points,
                MarkAsFailed = answer.MarkAsFailed,
                Color = answer.Color,
                Name = answer.Name,
            };
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

        public static AnswerButton[] CreatePassFailed()
        {
            var passed = new AnswerButton
            {
                Name = TemplateItemAnswer.Passed.Name,
                Color = TemplateItemAnswer.Passed.Color,
                Points = TemplateItemAnswer.Passed.Points,
                MarkAsFailed = false
            };

            var failed = new AnswerButton
            {
                Name = TemplateItemAnswer.Failed.Name,
                Color = TemplateItemAnswer.Failed.Color,
                Points = TemplateItemAnswer.Failed.Points,
                MarkAsFailed = true
            };

            return new[] { passed, failed };
        }
    }
}