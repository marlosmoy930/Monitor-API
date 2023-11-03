using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.Reports.Models.ExportToPdf
{
    public class SendEmailWidthExecutiveReportPdfRequest : GetExecutiveReportPdfRequest
    {
        public IEnumerable<Guid> UserIds { get; set; } = new List<Guid>();

        public IEnumerable<string> Emails { get; set; } = new List<string>();
    }
}