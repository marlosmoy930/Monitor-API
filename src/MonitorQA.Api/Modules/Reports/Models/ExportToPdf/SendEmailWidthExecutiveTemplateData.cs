using System.Collections.Generic;
using MonitorQA.Email.EmailMessages;
using MonitorQA.I18n.Properties;

namespace MonitorQA.Api.Modules.Reports.Models.ExportToPdf
{
    public class SendEmailWidthExecutiveTemplateData : GeneralEmailMessage.TemplateData
    {
        public string? CompanyName { get; set; }

        public static SendEmailWidthExecutiveTemplateData Create(
            string companyName,
            EmailRecipient recipient, 
            IEnumerable<EmailAttachment> attachments)
        {
            var templateData = new SendEmailWidthExecutiveTemplateData()
            {
                RecipientEmail = recipient.Email,
                RecipientName = recipient.Name,
                CompanyName = companyName,
            };
            templateData.SetSubjectFromI18nKey(nameof(I18nResources.ExecutiveReport_ExportPdf_Email_Subject));
            templateData.SetBodyFromI18nKey(nameof(I18nResources.ExecutiveReport_ExportPdf_Email_Body));
            templateData.EmailAttachments = attachments;

            return templateData;
        }
    }
}