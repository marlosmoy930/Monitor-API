using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes
{
    public class ChecklistAnswer : AnswerDataBase
    {
        public int? Points { get; set; }

        public bool MarkAsFailed { get; set; }

        public string Name { get; set; }

        public static ChecklistAnswer Create(TemplateItemAnswer answer)
        {
            return new ChecklistAnswer
            {
                Points = answer.Points,
                MarkAsFailed = answer.MarkAsFailed,
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
                Name = Name,
            };
        }

        public static ChecklistAnswer[] CreateAnswerType()
        {
            return new[]
            {
                new ChecklistAnswer
                {
                    Name = TemplateItemAnswer.ChecklistItems.CompleteName,
                    Points = 0,
                    MarkAsFailed = false
                },
                new ChecklistAnswer
                {
                    Name = TemplateItemAnswer.ChecklistItems.IncompleteName,
                    Points = 0,
                    MarkAsFailed = true
                }
            };
        }
    }
}
