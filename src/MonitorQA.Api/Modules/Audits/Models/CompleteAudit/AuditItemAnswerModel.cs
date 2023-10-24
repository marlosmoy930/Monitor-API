namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class AuditItemAnswerModel
    {
        public bool Selected { get; set; }

        public decimal? Number { get; set; }

        public decimal? FromNumber { get; set; }

        public decimal? ToNumber { get; set; }

        public int? Points { get; set; }

        public bool MarkAsFailed { get; set; }

        public string? Name { get; set; }

        public string? Color { get; set; }

        public int Index { get; set; }

        public string? Text { get; set; }
    }
}