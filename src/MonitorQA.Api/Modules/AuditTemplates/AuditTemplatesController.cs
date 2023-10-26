using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.AuditTemplates.Import;
using MonitorQA.Api.Modules.AuditTemplates.Models;
using MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes;
using MonitorQA.Api.Modules.AuditTemplates.Models.TemplateItem.Request;
using MonitorQA.Api.Modules.AuditTemplates.Models.TemplateItem.Response;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.AuditTemplates
{
    [Route("audit/templates")]
    [ApiController]
    public class AuditTemplatesController : AuthorizedController
    {
        private readonly SiteContext _context;

        public AuditTemplatesController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("concise")]
        public async Task<IActionResult> GetConciseList()
        {
            var list = await _context.Templates
                .AsNoTracking()
                .Where(Template.Predicates.BelongsToUserCompany(CurrentUser))
                .Where(Template.Predicates.IsNotDeleted())
                .Where(Template.Predicates.IsNotDraft())
                .Select(t => new
                {
                    t.Id,
                    t.Name
                })
                .OrderBy(t => t.Name)
                .ToListAsync();

            return Ok(list);
        } 

        [HttpGet]
        [Route("detailed")]
        public async Task<ListPageResult<DetailedTemplateModel>> GetDetailedList([FromQuery] DetailedListRequest model)
        {
            var query = model.Filter(_context.Templates.AsNoTracking(), CurrentUser.CompanyId);

            var count = await query.CountAsync();

            query = model.ApplyOrder(query);

            var items = await query
                .Skip(model.GetSkipNumber())
                .Take(model.PageSize)
                .Select(DetailedTemplateModel.GetExpression())
                .ToListAsync();

            var result = ListPageResult<DetailedTemplateModel>.Create(items, count, model);
            return result;
        }

        [HttpGet]
        [Route("answer-types")]
        public IActionResult GetAnswers()
        {
            return Ok(new object[]
            {
                new
                {
                    AnswerType = AnswerType.Buttons,
                    Data = AnswerButton.CreatePassFailed()
                },
                new
                {
                    AnswerType = AnswerType.YesNoButtons,
                    Data = YesNoButtonsAnswer.CreateYesNo()
                },
                new
                {
                    AnswerType = AnswerType.Numeric,
                    Data = NumericAnswer.CreateAnswerType()
                },
                new
                {
                    AnswerType = AnswerType.Checklist,
                    Data = ChecklistAnswer.CreateAnswerType()
                },
                new
                {
                    AnswerType = AnswerType.Text,
                    Data = new { text = "" }
                },
            });
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var companyId = CurrentUser.CompanyId;

            var templateItems = await _context.TemplateItems
                .AsNoTracking()
                .Where(x => x.TemplateId == id
                        && !x.IsDeleted
                        && x.Template.CompanyId == companyId)
                .Include(x => x.Answers)
                .Include(ti => ti.Tags).ThenInclude(tit => tit.Tag)
                .Include(ti => ti.InformationPhotos)
                .OrderBy(ti => ti.Index)
                .ToListAsync();

            var result = await _context.Templates
                .AsNoTracking()
                .Where(t => t.Id == id && !t.IsDeleted && t.CompanyId == companyId)
                .Select(t => new GetTemplateModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    IsDraft = t.IsDraft,
                    TemplateType = t.TemplateType,
                    IsAuditorSignatureRequired = t.IsAuditorSignatureRequired,
                    IsAuditeeSignatureRequired = t.IsAuditeeSignatureRequired,
                    SignatureAgreement = t.SignatureAgreement,
                    ScoreSystem = new IdNamePairModel<Guid>
                    {
                        Id = t.ScoreSystem.Id,
                        Name = t.ScoreSystem.Name
                    },
                    ConditionalTagsEnabled = t.TemplateItems.Any(i => i.Tags.Any()),
                    Tags = t.TemplateTags.Select(ut => new IdNamePairModel<Guid> { Id = ut.Tag.Id, Name = ut.Tag.Name }),
                    Data = new Dictionary<Guid, TemplateItemResponseModel>()
                })
                .FirstOrDefaultAsync();

            foreach (var templateItem in templateItems)
                result.Data.Add(templateItem.Id, new TemplateItemResponseModel(templateItem, templateItems));

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TemplateModel model)
        {
            var template = new Template
            {
                CompanyId = CurrentUser.CompanyId,
                Description = model.Description,
                Name = model.Name,
                IsDraft = model.IsDraft,
                TemplateType = model.TemplateType,
                IsAuditorSignatureRequired = model.IsAuditorSignatureRequired,
                IsAuditeeSignatureRequired = model.IsAuditeeSignatureRequired,
                ScoreSystemId = model.ScoreSystemId
            };

            if (model.TagsIds != null)
            {
                template.TemplateTags = model.TagsIds.Select(tid => new TemplateTag
                {
                    TagId = tid
                }).ToList();
            }

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            template.TemplateItems = new List<TemplateItem>
            {
                new TemplateItem
                {
                    Id = template.Id,
                    ItemType = ItemType.Root,
                    Text = "Root"
                }
            };

            await _context.SaveChangesAsync();

            return Ok(new IdModel { Id = template.Id });
        }

        [HttpPut]
        [Route("{templateId:Guid}")]
        public async Task<IActionResult> Update(Guid templateId, [FromBody] TemplateModel model)
        {
            var template = await _context.Templates
                .AsQueryable()
                .Include(t => t.TemplateTags)
                .FirstOrDefaultAsync(t => t.CompanyId == CurrentUser.CompanyId && t.Id == templateId);

            template.Name = model.Name;
            template.Description = model.Description;
            template.IsDraft = model.IsDraft;
            template.TemplateType = model.TemplateType;
            template.IsAuditorSignatureRequired = model.IsAuditorSignatureRequired;
            template.IsAuditeeSignatureRequired = model.IsAuditeeSignatureRequired;
            template.SignatureAgreement = model.SignatureAgreement;
            template.ScoreSystemId = model.ScoreSystemId;

            if (model.TagsIds != null)
                template.TemplateTags = model.TagsIds.Select(tid => new TemplateTag { TagId = tid }).ToList();

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(IdsArrayModel model)
        {
            var schedule = await _context.AuditSchedules
                .AsQueryable()
                .Where(AuditSchedule.Predicates.UserHasAccess(CurrentUser))
                .FirstOrDefaultAsync(s => model.Ids.Contains(s.TemplateId) && !s.IsDeleted);

            if (schedule != null)
                return Conflict("template/has-attached-schedules");

            var companyId = CurrentUser.CompanyId;
            var templates = await _context.Templates.AsQueryable()
                .Where(t => t.CompanyId == companyId && model.Ids.Any(id => id == t.Id))
                .ToListAsync();

            templates.ForEach(u => u.IsDeleted = true);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("{templateId:Guid}/items/{itemId}")]
        public async Task<IActionResult> SetTemplateBuilderItem(Guid templateId, Guid itemId, TemplateItemRequestModel requestModel)
        {
            var template = await _context.Templates
                .AsQueryable()
                .Include(t => t.TemplateItems).ThenInclude(ti => ti.Children)
                .Include(t => t.TemplateItems).ThenInclude(ti => ti.Answers)
                .Include(t => t.TemplateItems).ThenInclude(ti => ti.Tags)
                .Include(t => t.TemplateItems).ThenInclude(ti => ti.InformationPhotos)
                .FirstOrDefaultAsync(t => t.Id == templateId && t.CompanyId == CurrentUser.CompanyId);

            var templateItems = template.TemplateItems.ToList();
            var item = templateItems.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                item = new TemplateItem
                {
                    Id = itemId,
                    TemplateId = templateId,
                    ParentId = requestModel.ParentId,
                };
                template.TemplateItems.Add(item);
            }

            requestModel.UpdateEntity(item, templateItems);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        [Route("{templateId:Guid}/items/{itemId}")]
        public async Task<IActionResult> DeleteTemplateItem(Guid templateId, Guid itemId)
        {
            var companyId = CurrentUser.CompanyId;
            var items = await _context.TemplateItems.AsQueryable()
                .Where(ti => ti.TemplateId == templateId && ti.Template.CompanyId == companyId)
                .ToListAsync();

            if (!items.Any())
            {
                return Ok();
            }

            var item = items.Single(x => x.Id == itemId);

            item.DeleteSelfAndDecendants(items);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var companyId = CurrentUser.CompanyId;
            var importer = new TemplateImporter(_context);

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var templates = await importer.Import(companyId, stream);

            var templateIds = templates.Select(t => t.Id);
            return Ok(templateIds);
        }

    }
}