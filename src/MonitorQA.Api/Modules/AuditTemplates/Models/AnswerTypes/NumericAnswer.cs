using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes
{
    public class NumericAnswer : AnswerDataBase
    {
        public int? Points { get; set; }

        public decimal? FromNumber { get; set; }

        public decimal? ToNumber { get; set; }

        public bool MarkAsFailed { get; set; }

        public static NumericAnswer Create(TemplateItemAnswer answer)
        {
            return new NumericAnswer
            {
                Points = answer.Points,
                MarkAsFailed = answer.MarkAsFailed,
                FromNumber = answer.FromNumber,
                ToNumber = answer.ToNumber,
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
                FromNumber = FromNumber,
                ToNumber = ToNumber
            };
        }

        public static NumericAnswer[] CreateAnswerType()
        {
            return new[]
            {
                new NumericAnswer
                {
                    FromNumber = 0,
                    ToNumber = 0,
                    Points = 0
                }
            };
        }
    }
}