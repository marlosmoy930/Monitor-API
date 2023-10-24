using MonitorQA.Api.Infrastructure.Models;
using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.Audits.Models.CompleteAudit
{
    public class CorrectiveActionCommentModel
    {
        public Guid Id { get; set; }

        public string Body { get; set; }

        public IdModel CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public IEnumerable<PhotoModel> Photos { get; set; } = new List<PhotoModel>();
    }
}