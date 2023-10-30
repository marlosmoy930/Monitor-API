using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Api.Modules.Reports.Models.SectionsPerformance;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using MonitorQA.Pdf.Reports.Executive.Models;
using MonitorQA.Pdf.Reports.Executive.Models.SectionPerformance;

namespace MonitorQA.Api.Modules.Reports.Models
{
    public class CompareItem<T> where T: ICloneable
    {
        public Guid Id { get; set; }

        public CompareType CompareType { get; set; }

        public List<T>? Items { get; set; }

        public bool HasAudit(Audit audit)
        {
            if (CompareType == CompareType.User)
            {
                return audit.AuditObject.AuditObjectUsers.Any(aou => aou.UserId == Id);
            }
            else if (CompareType == CompareType.UserGorup)
            {
                return audit.AuditObject.AuditObjectUsers.Any(aou => aou.UserGroupId.HasValue && aou.UserGroupId.Value == Id);
            }
            else if (CompareType == CompareType.AuditObject)
            {
                return audit.AuditObjectId == Id;
            }
            else if (CompareType == CompareType.AuditObjectGroup)
            {
                return audit.AuditObject.AuditObjectAuditObjectGroups.Any(aoaog => aoaog.AuditObjectGroupId == Id);
            }

            throw new NotImplementedException($"{nameof(Models.CompareType)} {CompareType}");
        }

        internal PdfReportCompareItem<PdfReportSectionPerformanceItem> GetPdfReportSectionItem(List<IdNamePairModel<Guid>> idNamePairs)
        {
            return new PdfReportCompareItem<PdfReportSectionPerformanceItem>
            {
                Name = idNamePairs.First(p => p.Id == Id).Name,
                Items = Items
                    .Select(i => (i as SectionPerformanceItem)!.GetPdfReportSectionItem())
                    .ToList()
            };
        }
    }
}
