using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.Reports.Models;
using MonitorQA.Api.Modules.Reports.Models.AuditCompletion;
using MonitorQA.Api.Modules.Reports.Models.AuditDayOfWeekBreakdown;
using MonitorQA.Api.Modules.Reports.Models.AuditPerformance;
using MonitorQA.Api.Modules.Reports.Models.CorrectiveActionsAnalysis;
using MonitorQA.Api.Modules.Reports.Models.ExportToPdf;
using MonitorQA.Api.Modules.Reports.Models.Filters;
using MonitorQA.Api.Modules.Reports.Models.ScoreBreakdown;
using MonitorQA.Api.Modules.Reports.Models.SectionsPerformance;
using MonitorQA.Api.Modules.Reports.Models.Totals;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Email.EmailMessages;
using MonitorQA.Pdf.Reports.Executive;
using MonitorQA.Pdf.Reports.Executive.Models.ScoreBreakdown;
using PdfReportInfo = MonitorQA.Pdf.Reports.Executive.Models.PdfReportInfo;

namespace MonitorQA.Api.Modules.Reports
{
    [Route("reports")]
    [ApiController]
    public class ReportsController : AuthorizedController
    {
        private readonly SiteContext _context;
        private readonly CloudMessagePublisher _publisher;

        public ReportsController(SiteContext context, CloudMessagePublisher publisher) : base(context)
        {
            _context = context;
            _publisher = publisher;
        }

        [HttpPost]
        [Route("totals")]
        public async Task<TotalsResponse> GetTotals(ReportFilter filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredAuditsQuery = filter.GetPendingAndCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var previousAuditsQuery = ReportsUtil.GetPreviousAuditsQuery(filteredAuditsQuery);
            var completedAuditsQuery = filter.GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var previousCompletedAuditsQuery = ReportsUtil.GetPreviousCompletedAuditsQuery(filteredAuditsQuery);

            var auditTotal = new TotalAuditResponse();
            auditTotal.Total = await filteredAuditsQuery.CountAsync();
            auditTotal.Completed = await completedAuditsQuery.CountAsync();
            auditTotal.PreviousTotal = await previousAuditsQuery.CountAsync();
            auditTotal.PreviousCompleted = await previousCompletedAuditsQuery.CountAsync();

            var filteredActionsQuery = filter.GetFilteredActionsQuery(auditsQuery, CurrentUser.CompanyId);
            var approvedActionsQuery = TotalActionsResponse.GetAprrovedActionsQuery(filteredActionsQuery);
            var previousActionsQuery = TotalActionsResponse.GetActionsQuery(previousAuditsQuery);
            var previousApprovedActionsQuery = TotalActionsResponse.GetAprrovedActionsQuery(previousActionsQuery);

            var actionsTotal = new TotalActionsResponse();
            actionsTotal.Total = await filteredActionsQuery.CountAsync();
            actionsTotal.Approved = await approvedActionsQuery.CountAsync();
            actionsTotal.PreviousTotal = await previousActionsQuery.CountAsync();
            actionsTotal.PreviousApproved = await previousApprovedActionsQuery.CountAsync();

            var response = new TotalsResponse
            {
                Audits = auditTotal,
                Actions = actionsTotal,
            };

            return response;
        }

        [HttpPost]
        [Route("audit-compliance")]
        public async Task<AuditComplianceResponse> GetAuditCompliance(ReportFilter filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredAuditsQuery = filter.GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var score = await filteredAuditsQuery
                .Select(a => (decimal?)a.Score)
                .AverageAsync();

            var previousAuditsQuery = ReportsUtil.GetPreviousAuditsQuery(filteredAuditsQuery);
            var previousScore = await previousAuditsQuery
                .Select(a => (decimal?)a.Score)
                .AverageAsync();

            var scoreSystemElements = await _context.Templates
                .AsNoTracking()
                .Where(t => t.Id == filter.TemplateId)
                .SelectMany(t => t.ScoreSystem.ScoreSystemElements)
                .ToListAsync();

            var element = ScoreSystemElement.GetElementByScore(scoreSystemElements, score.GetValueOrDefault());
            var scoreColor = element.Color;

            var response = new AuditComplianceResponse()
            {
                Score = score.GetValueOrDefault(),
                PreviousScore = previousScore.GetValueOrDefault(),
                ScoreColor = scoreColor
            };

            return response;
        }

        [HttpPost]
        [Route("audit-performance")]
        public async Task<PerformanceResponse<AuditPerformanceItem>> GetAuditPerformance(ReportFilterWithCompare filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredAuditsQuery = filter.GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var auditInfoItems = await GetAuditInfoItems(filteredAuditsQuery);

            var previousAudits = await ReportsUtil
                .GetPreviousAuditsQuery(filteredAuditsQuery)
                .Include(x => x.AuditItems).ThenInclude(ai => ai.Answers)
                .ToListAsync();

            var performanceItems = auditInfoItems
                .GroupBy(i => i.Audit.CompleteDate!.Value.Date)
                .Select(g => new AuditPerformanceItem
                {
                    Date = g.Key,
                    AuditInfoItems = g.ToList(),
                })
                .ToList();

            var compareItems = filter.Compare?.GetCompareItems(performanceItems)
            ?? new List<CompareItem<AuditPerformanceItem>>();

            foreach (var performanceItem in performanceItems)
            {
                foreach (var auditInfoItem in performanceItem.AuditInfoItems!)
                {
                    var audit = auditInfoItem.Audit;
                    var auditScore = audit.Score;
                    performanceItem.AuditScores.Add(audit.Score);

                    if (auditInfoItem.PreviousAuditId.HasValue)
                    {
                        var previousAudit = previousAudits.Single(a => a.Id == auditInfoItem.PreviousAuditId);
                        performanceItem.PreviousAuditScores.Add(previousAudit.Score);
                    }

                    var auditCompareItems = compareItems
                        .Where(i => i.HasAudit(audit))
                        .ToList();

                    foreach (var auditCompareItem in auditCompareItems)
                    {
                        var auditComparePerformanceItem = auditCompareItem.Items.Single(i => i.Date == performanceItem.Date);
                        auditComparePerformanceItem.AuditScores.Add(auditScore);
                    }
                }

            }

            var response = new PerformanceResponse<AuditPerformanceItem>()
            {
                Items = performanceItems,
                Compare = compareItems,
            };

            return response;
        }

        [HttpPost]
        [Route("audit-completion")]
        public async Task<PerformanceResponse<AuditCompletionItem>> GetAuditCompletion(ReportFilterWithCompare filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var audits = await filter
                .GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId)
                .Include(a => a.AuditObject.AuditObjectUsers)
                .Include(a => a.AuditObject.AuditObjectAuditObjectGroups)
                .ToListAsync();

            var completionItems = audits
                .GroupBy(a => a.CompleteDate!.Value.Date)
                .Select(g => new AuditCompletionItem
                {
                    Date = g.Key,
                    Audits = g.ToList(),
                })
                .ToList();

            var compareItems = filter.Compare?.GetCompareItems(completionItems)
            ?? new List<CompareItem<AuditCompletionItem>>();

            foreach (var completionItem in completionItems)
            {
                foreach (var audit in completionItem.Audits!)
                {
                    var auditCompareItems = compareItems
                        .Where(i => i.HasAudit(audit))
                        .ToList();

                    foreach (var auditCompareItem in auditCompareItems)
                    {
                        var auditComparePerformanceItem = auditCompareItem.Items.Single(i => i.Date == completionItem.Date);
                        auditComparePerformanceItem.Audits.Add(audit);
                    }
                }

            }

            var response = new PerformanceResponse<AuditCompletionItem>()
            {
                Items = completionItems,
                Compare = compareItems,
            };

            return response;
        }

        [HttpPost]
        [Route("score-breakdown")]
        public async Task<IEnumerable<ScoreBreakdownItem>> GetScoreBreakdown(ReportFilter filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredAuditsQuery = filter.GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var auditInfos = await filteredAuditsQuery
                .Select(a => new AuditScoreInfo { AuditId = a.Id, Score = a.Score })
                .ToListAsync();

            var scoreSystemElements = await _context.Templates
                .AsNoTracking()
                .Where(t => t.Id == filter.TemplateId)
                .SelectMany(t => t.ScoreSystem.ScoreSystemElements)
                .ToListAsync();

            var items = scoreSystemElements
               .Select(ScoreBreakdownItem.Create)
               .ToList();

            foreach (var auditInfo in auditInfos)
            {
                items.TryAdd(auditInfo.Score);
            }

            return items;
        }

        [HttpPost]
        [Route("actions-analysis")]
        public async Task<CorrectiveActionsAnalysisResponse> GetCorrectiveActionsAnalysis(ReportFilter filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredActionsQuery = filter.GetFilteredActionsQuery(auditsQuery, CurrentUser.CompanyId);
            var actionInfos = await filteredActionsQuery
                .Select(a => new CorrectiveActionStatusInfo
                {
                    ActionId = a.Id,
                    Status = a.Status,
                    DueDate = a.DueDate,
                    ApprovedAt = a.ApprovedAt,
                })
                .ToListAsync();

            var response = new CorrectiveActionsAnalysisResponse();

            foreach (var actionInfo in actionInfos)
            {
                response.Total.TryAdd(actionInfo.Status);

                if (actionInfo.IsLate)
                    response.Late.TryAdd(actionInfo.Status);
            }

            return response;
        }

        [HttpPost]
        [Route("sections-performance")]
        public async Task<PerformanceResponse<SectionPerformanceItem>> GetSectionsPerformance(ReportFilterWithCompare filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredAuditsQuery = filter.GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var auditInfoItems = await GetAuditInfoItems(filteredAuditsQuery);

            var previousAudits = await ReportsUtil
                .GetPreviousAuditsQuery(filteredAuditsQuery)
                .Include(x => x.AuditItems).ThenInclude(ai => ai.Answers)
                .ToListAsync();

            var sections = await _context.Templates
                .AsNoTracking()
                .Where(t => t.Id == filter.TemplateId)
                .SelectMany(t => t.TemplateItems)
                .Where(ti => !ti.IsDeleted)
                .Where(ti => ti.ItemType == ItemType.Section)
                .Where(ti => ti.Parent.ItemType == ItemType.Root)
                .OrderBy(ti => ti.Index)
                .Select(ti => new SectionPerformanceItem
                {
                    TemplateItemId = ti.Id,
                    Name = ti.Text
                })
                .ToListAsync();

            var compareItems = filter.Compare?.GetCompareItems(sections)
                ?? new List<CompareItem<SectionPerformanceItem>>();

            foreach (var auditInfoItem in auditInfoItems)
            {
                var audit = auditInfoItem.Audit;
                audit.CalculateAndSetScore();

                var previousAudit = auditInfoItem.PreviousAuditId.HasValue
                    ? previousAudits.Single(a => a.Id == auditInfoItem.PreviousAuditId)
                    : null;
                previousAudit?.CalculateAndSetScore();

                var auditCompareItems = compareItems
                    .Where(i => i.HasAudit(audit))
                    .ToList();

                foreach (var section in sections)
                {
                    var auditItem = audit.AuditItems
                        .SingleOrDefault(ai => ai.TemplateItemId == section.TemplateItemId);

                    if (auditItem != null)
                    {
                        var auditScore = auditItem.GetScore();
                        section.AuditScores.Add(auditScore);

                        foreach (var auditCompareItem in auditCompareItems)
                        {
                            var auditCompareSection = auditCompareItem.Items.Single(s => s.TemplateItemId == section.TemplateItemId);
                            auditCompareSection.AuditScores.Add(auditScore);
                        }
                    }

                    if (previousAudit != null)
                    {
                        var previousAuditItem = previousAudit.AuditItems
                            .SingleOrDefault(ai => ai.TemplateItemId == section.TemplateItemId);

                        if (previousAuditItem != null)
                        {
                            var previousAuditScore = previousAuditItem.GetScore();
                            section.PreviousAuditScores.Add(previousAuditScore);

                            foreach (var auditCompareItem in auditCompareItems)
                            {
                                var auditCompareSection = auditCompareItem.Items.Single(s => s.TemplateItemId == section.TemplateItemId);
                                auditCompareSection.PreviousAuditScores.Add(previousAuditScore);
                            }
                        }
                    }
                }
            }

            var response = new PerformanceResponse<SectionPerformanceItem>()
            {
                Items = sections,
                Compare = compareItems
                    .Where(i => i.Items.All(ii => ii.AuditScores.Any()))
                    .ToList(),
            };
            return response;
        }

        [HttpPost]
        [Route("audit-day-of-week-breakdown")]
        public async Task<IEnumerable<AuditDayOfWeekBreakdownItem>> GetAuditDayOfWeekBreakdown(ReportFilter filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredAuditsQuery = filter.GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var items = await filteredAuditsQuery
                .Select(a => new AuditDayOfWeekBreakdownInfo
                {
                    AuditId = a.Id,
                    CompleteDate = a.CompleteDate.Value
                })
                .ToListAsync();

            var groupedItems = items
                .GroupBy(i => i.DayOfWeek)
                .Select(g => new AuditDayOfWeekBreakdownItem
                {
                    DayOfWeek = g.Key,
                    Count = g.Count(),
                })
                .ToList();

            return groupedItems;
        }

        [HttpPost]
        [Route("audit-completion-time")]
        public async Task<IEnumerable<int?>> GetAuditCompletionTime(ReportFilter filter)
        {
            var auditsQuery = _context.Audits.AsNoTracking();
            var filteredAuditsQuery = filter.GetFilteredCompletedAuditsQuery(auditsQuery, CurrentUser.CompanyId);
            var items = await filteredAuditsQuery
                .Select(a => EF.Functions.DateDiffSecond(a.StartedAt, a.CompleteDate))
                .ToListAsync();

            return items;
        }

        [HttpPost]
        [Route("export-pdf")]
        public async Task<FileContentResult> GetReportPdf(GetExecutiveReportPdfRequest request)
        {
            var report = await GetExecutiveReport(request);
            var (reportBytes, fileType, fileName) = report.GetFileInfo();

            return File(reportBytes, fileType, fileName);
        }

        [HttpPost]
        [Route("email-pdf")]
        public async Task SendReportPdf(SendEmailWidthExecutiveReportPdfRequest request)
        {
            var recipients = request.Emails
                .Select(e => new EmailRecipient() { Email = e })
                .ToList();
            if (request.UserIds.Any())
            {
                var userRecipients = await _context.Users
                    .Where(u => request.UserIds.Contains(u.Id))
                    .Select(u => new EmailRecipient() { Email = u.Email, Name = u.Name })
                    .ToListAsync();
                recipients.AddRange(userRecipients);
            }

            if (!recipients.Any()) return;

            var companyName = await _context.Companies
                .Where(c => c.Id == CurrentUser.CompanyId)
                .Select(c => c.Name)
                .SingleAsync();

            var report = await GetExecutiveReport(request);
            var (reportBytes, fileType, fileName) = report.GetFileInfo();

            var publishTasks = new List<Task>();
            var attachments = new List<EmailAttachment>()
            {
                EmailAttachment.Create(fileName, reportBytes, fileType)
            };

            foreach (var recipient in recipients)
            {
                var templateData = SendEmailWidthExecutiveTemplateData.Create(companyName, recipient, attachments);
                var message = new GeneralEmailMessage { Data = templateData };
                publishTasks.Add(_publisher.Publish(message));
            }

            await Task.WhenAll(publishTasks);
        }

        private async Task<List<IdNamePairModel<Guid>>> GetSectionIdNamePairs(PerformanceResponse<SectionPerformanceItem> sectionPerformance)
        {
            var idNamePairs = new List<IdNamePairModel<Guid>>();

            var sectionUserIds = sectionPerformance.Compare
                .Where(i => i.CompareType == CompareType.User)
                .Select(i => i.Id)
                .ToList();
            idNamePairs.AddRange(await _context.Users
                .Where(u => sectionUserIds.Contains(u.Id))
                .Select(u => new IdNamePairModel<Guid> { Id = u.Id, Name = u.Name })
                .ToListAsync());

            var sectionUserGroupIds = sectionPerformance.Compare
                .Where(i => i.CompareType == CompareType.UserGorup)
                .Select(i => i.Id)
                .ToList();
            idNamePairs.AddRange(await _context.UserGroups
                .Where(ug => sectionUserGroupIds.Contains(ug.Id))
                .Select(ug => new IdNamePairModel<Guid> { Id = ug.Id, Name = ug.Name })
                .ToListAsync());

            var sectionAuditObjectIds = sectionPerformance.Compare
                .Where(i => i.CompareType == CompareType.AuditObject)
                .Select(i => i.Id)
                .ToList();
            idNamePairs.AddRange(await _context.AuditObjects
                .Where(ug => sectionAuditObjectIds.Contains(ug.Id))
                .Select(ug => new IdNamePairModel<Guid> { Id = ug.Id, Name = ug.Name })
                .ToListAsync());

            var sectionAuditObjectGroupIds = sectionPerformance.Compare
                .Where(ao => ao.CompareType == CompareType.AuditObjectGroup)
                .Select(ao => ao.Id)
                .ToList();
            idNamePairs.AddRange(await _context.AuditObjectGroups
                .Where(aog => sectionAuditObjectGroupIds.Contains(aog.Id))
                .Select(aog => new IdNamePairModel<Guid> { Id = aog.Id, Name = aog.Name })
                .ToListAsync());

            return idNamePairs;
        }

        private async Task<ExecutiveReport> GetExecutiveReport(GetExecutiveReportPdfRequest request)
        {
            var scoreBreakdownItems = (await GetScoreBreakdown(request.Filter))
                .Select(i => i.ToPdfReportItem());

            var sectionPerformanceFilter = ReportFilterWithCompare.Create(request.Filter, request.SectionPerformanceCompare);
            var sectionPerformance = await GetSectionsPerformance(sectionPerformanceFilter);
            var idNamePairs = await GetSectionIdNamePairs(sectionPerformance);

            var info = new PdfReportInfo()
            {
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CompanyName = await _context.Companies
                    .Where(c => c.Id == CurrentUser.CompanyId)
                    .Select(c => c.Name)
                    .SingleAsync(),
                CreatedBy = await _context.Users
                    .Where(u => u.Id == CurrentUser.Id)
                    .SingleAsync(),
                Filter = request.Filter.GetPdfReportFilter(),
                Totals = (await GetTotals(request.Filter!)).GetPdfReportTotals(),
                AuditCompliance = (await GetAuditCompliance(request.Filter!)).GetPdfReportAuditCompliance(),
                ScoreBreakdown = PdfReportScoreBreakdown.Create(request.ScoreBreakdownSvg, scoreBreakdownItems),
                CorrectiveActionsAnalysis = (await GetCorrectiveActionsAnalysis(request.Filter))
                    .GetPdfReportCorrectiveActionsAnalysis(request.CorrectiveActionsAnalysisTotalSvg, request.CorrectiveActionsAnalysisLateSvg),
                SectionPermance = sectionPerformance.GetPdfReportSectionPermance(idNamePairs),
                AuditPermanceSvg = request.AuditPerformanceSvg,
                AuditCompletionSvg = request.AuditCompletionSvg,
                AuditDayOfWeekBreakdownSvg = request.AuditDayOfWeekBreakdownSvg,
                AuditCompletionTimeSvg = request.AuditCompletionTimeSvg,
            };

            info.Filter.TemplateName = await _context.Templates
                    .Where(t => t.Id == request.Filter.TemplateId)
                    .Select(t => t.Name)
                    .SingleAsync();

            if (request.Filter.AuditObjectId.HasValue)
            {
                info.Filter.AuditObjectNames = await _context.AuditObjects
                    .Where(t => t.Id == request.Filter.AuditObjectId)
                    .Select(t => t.Name)
                    .SingleAsync();
            }

            var userIds = request.Filter.UserIds ?? new List<Guid>();
            var userGroupIds = request.Filter.UserGroupIds ?? new List<Guid>();
            if (userIds.Any() || userGroupIds.Any())
            {
                info.Filter.Participants = await _context.Users
                    .Where(u => userIds.Contains(u.Id)
                            || u.UserUserGroups.Any(uug => userGroupIds.Contains(uug.UserGroupId)))
                    .Select(u => u.Name)
                    .ToListAsync();
            }

            if (request.Filter?.TagsIds.Any() == true)
            {
                info.Filter.TagsNames = await _context.Tags
                    .Where(t => request.Filter.TagsIds!.Contains(t.Id))
                    .Select(u => u.Name)
                    .ToListAsync();
            }

            var report = new ExecutiveReport(info);
            return report;
        }

        private async Task<List<AuditInfoItem>> GetAuditInfoItems(IQueryable<Audit> filteredAuditsQuery)
        {
            var auditInfoItems = await filteredAuditsQuery
                .Select(a => new AuditInfoItem
                {
                    Audit = a,
                    PreviousAuditId = a.AuditObject.Audits
                        .Where(a2 => !a2.IsDeleted)
                        .Where(a2 => a2.AuditObjectId == a.AuditObjectId)
                        .Where(a2 => a2.AuditSchedule.TemplateId == a.AuditSchedule.TemplateId)
                        .Where(a2 => a2.IsCompleted)
                        .Where(a2 => !a.CompleteDate.HasValue || a2.CompleteDate < a.CompleteDate)
                        .OrderByDescending(a2 => a2.CompleteDate)
                        .Select(a2 => a2.Id)
                        .Take(1)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var auditIds = auditInfoItems
                .Select(a => a.Audit.Id)
                .ToList();

            var auditItems = await _context.AuditItems
                .AsNoTracking()
                .Where(ai => auditIds.Contains(ai.AuditId))
                .Include(ai => ai.Answers)
                .ToListAsync();

            var auditObjectIds = auditInfoItems
                .Select(a => a.Audit.AuditObjectId)
                .ToList();

            var auditObjects = await _context.AuditObjects
                .AsNoTracking()
                .Where(ao => auditObjectIds.Contains(ao.Id))
                .Include(ao => ao.AuditObjectUsers)
                .Include(ao => ao.AuditObjectAuditObjectGroups)
                .ToListAsync();

            foreach (var auditInfoItem in auditInfoItems)
            {
                auditInfoItem.Audit.AuditItems = auditItems
                    .Where(ai => ai.AuditId == auditInfoItem.Audit.Id)
                    .ToList();

                auditInfoItem.Audit.AuditObject = auditObjects
                    .SingleOrDefault(ao => ao.Id == auditInfoItem.Audit.AuditObjectId);
            }

            return auditInfoItems;
        }
    }
}
