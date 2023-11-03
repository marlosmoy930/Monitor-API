namespace MonitorQA.Api.Modules.Reports.Models.Filters
{
    public class ReportFilterWithCompare : ReportFilter
    {
        public ReportCompare? Compare { get; set; }

        public static ReportFilterWithCompare Create(ReportFilter filter, ReportCompare? compare)
        {
            return new ReportFilterWithCompare
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TemplateId = filter.TemplateId,
                AuditObjectId = filter.AuditObjectId,
                UserIds = filter.UserIds,
                UserGroupIds = filter.UserGroupIds,
                TagsIds = filter.TagsIds,

                Compare = compare
            };
        }
    }
}
