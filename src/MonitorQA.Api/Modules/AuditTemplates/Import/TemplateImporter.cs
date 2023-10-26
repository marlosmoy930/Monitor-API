using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.AuditTemplates.Import
{
    public class TemplateImporter
    {
        private readonly SiteContext _context;

        public TemplateImporter(SiteContext context)
        {
            this._context = context;
        }

        public async Task<List<Template>> Import(Guid companyId, Stream stream)
        {
            var company = await _context.Companies.SingleAsync(c => c.Id == companyId);
            var defaultScoreSystem = await _context.ScoreSystems.FirstAsync(s => s.CompanyId == companyId);

            using var workbook = new XLWorkbook(stream);

            var templates = workbook.Worksheets
                .Select(s => GetTemplateFromWorksheet(company, defaultScoreSystem, s))
                .ToList();

            _context.Templates.AddRange(templates);

            await _context.SaveChangesAsync();

            return templates;
        }

        private static Template GetTemplateFromWorksheet(Company company, ScoreSystem scoreSystem, IXLWorksheet worksheet)
        {
            var worksheetWithTemplate = new WorksheetWithTemplateToImport(worksheet);
            var template = worksheetWithTemplate.GetTemplate(company, scoreSystem);
            return template;
        }
    }

}
