using ClosedXML.Excel;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.AuditTemplates.Import
{
    public class WorksheetWithTemplateToImport
    {
        const string templateNameCellAddress = "B1";
        const int firstColumnIndex = 1;

        public const int HeaderRowNumber = 2;

        private readonly IXLWorksheet _worksheet;

        public WorksheetWithTemplateToImport(IXLWorksheet xLWorksheet)
        {
            this._worksheet = xLWorksheet;
        }

        public Template GetTemplate(Company company, ScoreSystem scoreSystem)
        {
            var templateId = Guid.NewGuid();

            var root = GetRoot(templateId);

            var template = new Template()
            {
                Id = templateId,
                Name = GetTemplateName(),
                CompanyId = company.Id,
                Availability = TemplateAvailability.Private,
                TemplateItems = new List<TemplateItem> { root },
                ScoreSystemId = scoreSystem.Id
            };
            return template;
        }

        private TemplateItem GetRoot(Guid templateId)
        {
            var startCell = _worksheet.Cell(HeaderRowNumber + 1, firstColumnIndex);
            var root = XlTemplateSection.CreateRoot(startCell, templateId);
            return root;
        }

        private string GetTemplateName()
        {
            var name = _worksheet
                .Cell(templateNameCellAddress)
                .GetString()
                .Trim();
            return name;
        }

    }
}
