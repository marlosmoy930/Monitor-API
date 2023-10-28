using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.Libraries
{
    [Route("library")]
    [ApiController]
    public class LibraryController : AuthorizedController
    {
        private readonly SiteContext _context;

        public LibraryController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _context.Templates
                .AsNoTracking()
                .Where(x => x.Availability == TemplateAvailability.Public)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                })
                .ToListAsync();

            return Ok(templates);
        }

        [HttpPost]
        [Route("templates/copy")]
        public async Task<IActionResult> CopyTemplates([FromBody] IdsArrayModel model)
        {
            var companyId = CurrentUser.CompanyId;
            var defaultScoreSystem = await _context.ScoreSystems.FirstAsync(s => s.CompanyId == companyId);

            var templates = await _context.Templates
                .AsNoTracking()
                .Where(x => !x.IsDeleted
                        && x.Availability == TemplateAvailability.Public
                        && model.Ids.Contains(x.Id))
                .Include(x => x.TemplateItems).ThenInclude(x => x.Answers)
                .ToListAsync();

            var companyTemplates = new List<Template>();
            foreach (var template in templates)
            {
                template.Id = Guid.NewGuid();
                template.CompanyId = companyId;
                template.Availability = TemplateAvailability.Private;
                template.ScoreSystemId = defaultScoreSystem.Id;

                template.CloneTemplateItems();

                companyTemplates.Add(template);
            }

            _context.AddRange(companyTemplates);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
