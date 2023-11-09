using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Tags
{
    [Route("[controller]")]
    public class TagsController : AuthorizedController
    {
        private readonly SiteContext _context;

        public TagsController(SiteContext context)
            : base(context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("detailed")]
        public async Task<IActionResult> GetDetailed([FromQuery] ListRequestModel model)
        {
            var query = _context.Tags
                .AsNoTracking()
                .Where(t => !t.IsDeleted)
                .Where(r => r.CompanyId == CurrentUser.CompanyId);

            if (!string.IsNullOrEmpty(model.Search))
                query = query.Where(u => u.Name.Contains(model.Search));

            var count = await query.CountAsync();

            query = model.OrderByDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(t => t.Name)
                : query.OrderBy(t => t.Name);

            var data = query.Select(tag => new
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Color = tag.Color,
                    Description = tag.Description
                })
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize);

            return Ok(new
            {
                Data = data,
                Meta = new PagingModel {PageNumber = model.PageNumber, PageSize = model.PageSize, TotalCount = count}
            });
        }

        [HttpGet]
        [Route("concise")]
        public async Task<IActionResult> GetConcise()
        {
            var list = await _context.Tags
                .AsNoTracking()
                .Where(t => !t.IsDeleted)
                .Where(r => r.CompanyId == CurrentUser.CompanyId)
                .Select(tag => new
                {
                    tag.Id,
                    tag.Name,
                    tag.Color
                })
                .OrderBy(tag => tag.Name)
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TagModel model)
        {
            var tag = new Tag
            {
                Name = model.Name,
                CompanyId = CurrentUser.CompanyId,
                Color = model.Color,
                Description = model.Description
            };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return Ok(new IdModel {Id = tag.Id});
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TagModel model)
        {
            var tag = await _context.Tags.AsQueryable()
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == CurrentUser.CompanyId);

            tag.Name = model.Name;
            tag.Color = model.Color;
            tag.Description = model.Description;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel model)
        {
            var tags = await _context.Tags
                .AsQueryable()
                .Where(t => t.CompanyId == CurrentUser.CompanyId)
                .Where(t => model.Ids.Contains(t.Id))
                .Where(t => !t.UserTags.Any())
                .Where(t => !t.AuditObjectTags.Any())
                .Where(t => !t.AuditScheduleTags.Any() 
                        || t.AuditScheduleTags.All(s => s.AuditSchedule.Audits.All(a => a.IsCompleted)))
                .Where(t => !t.TemplateTags.Any())
                .ToListAsync();

            var notDeletedTags = model.Ids
                .Where(id => !tags.Any(t => t.Id == id))
                .ToList();

            if (notDeletedTags.Any())
            {
                return Conflict("tags/has-attached-entities");
            }

            foreach (var tag in tags)
            {
                tag.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}