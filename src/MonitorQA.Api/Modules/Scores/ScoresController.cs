using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.Scores.Models;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Scores
{
    [Route("[controller]")]
    [ApiController]
    public class ScoresController : AuthorizedController
    {
        private readonly SiteContext _context;

        public ScoresController(SiteContext context) : base(context)
        {
            _context = context;
        }


        [HttpGet]
        [Route("concise")]
        public async Task<IActionResult> GetConciseList()
        {
            var data = await _context.ScoreSystems
                .AsNoTracking()
                .Where(ScoreSystem.Predicates.NotDeleted())
                .Where(ScoreSystem.Predicates.SameCompany(CurrentUser))
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet]
        [Route("detailed")]
        public async Task<IActionResult> GetList([FromQuery] ScoreListRequestModel model)
        {
            var query = model.Filter(_context.ScoreSystems.AsNoTracking(), CurrentUser);

            var count = await query.CountAsync();

            query = model.ApplyOrder(query);

            var data = await query
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.PassedThreshold,
                    Labels = s.ScoreSystemElements.Select(l => new
                    {
                        l.Id,
                        l.Label,
                        l.Min,
                        l.Max,
                        l.Color
                    })
                })
                .ToListAsync();

            return Ok(new ListPageResult(data, count, model));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScoreSystemModel model)
        {
            if (!CurrentUser.Role.CanManageScoreSystems)
                return Forbid();

            _context.ScoreSystems.Add(new ScoreSystem
            {
                CompanyId = CurrentUser.CompanyId,
                Name = model.Name,
                Description = model.Description,
                PassedThreshold = model.PassedThreshold,
                ScoreSystemElements = model.Labels.Select(l => new ScoreSystemElement
                {
                    Label = l.Label,
                    Max = l.Max,
                    Min = l.Min,
                    Color = l.Color
                }).ToList()
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ScoreSystemModel model)
        {
            if(!CurrentUser.Role.CanManageScoreSystems)
                return Forbid();

            var scoreSystem = await _context.ScoreSystems
                .Include(s => s.ScoreSystemElements)
                .Where(ScoreSystem.Predicates.SameCompany(CurrentUser))
                .SingleAsync(ScoreSystem.Predicates.WithId(id));

            scoreSystem.Name = model.Name;
            scoreSystem.Description = model.Description;
            scoreSystem.PassedThreshold = model.PassedThreshold;
            scoreSystem.ScoreSystemElements = model.Labels.Select(l => new ScoreSystemElement
            {
                Label = l.Label,
                Max = l.Max,
                Min = l.Min,
                Color = l.Color
            }).ToList();

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel<Guid> model)
        {
            if (!CurrentUser.Role.CanManageScoreSystems)
                return Forbid();

            var scoreSystems = await _context.ScoreSystems
                .Where(ScoreSystem.Predicates.SameCompany(CurrentUser))
                .Where(ScoreSystem.Predicates.NotDeleted())
                .Where(ScoreSystem.Predicates.WithIds(model.Ids))
                .Include(s => s.Templates)
                .ToListAsync();

            var systemsWithTemplates = new List<Guid>();
            foreach (var scoreSystem in scoreSystems)
            {
                if (scoreSystem.Templates.Any())
                {
                    systemsWithTemplates.Add(scoreSystem.Id);
                }
                else
                {
                    scoreSystem.IsDeleted = true;
                }
            }

            await _context.SaveChangesAsync();

            if (systemsWithTemplates.Any())
            {
                throw BusinessException.ScoreSystemExceptions.GetScroreSystemHasTemplates(systemsWithTemplates);
            }

            return Ok();
        }
    }
}