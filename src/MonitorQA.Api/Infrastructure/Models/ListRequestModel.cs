using System;

namespace MonitorQA.Api.Infrastructure.Models
{
    public class ListRequestModel : IPagingRequestModel
    {
        public const string AscendingOrderDirection = "asc";
        
        public const string DescendingOrderDirection = "desc";

        protected const string OrderByNameFieldName = "name";

        public string? Search { get; set; }

        public string OrderBy { get; set; } = OrderByNameFieldName;

        public string OrderByDirection { get; set; } = AscendingOrderDirection;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        protected bool IsAscendingOrderDirection 
            => AscendingOrderDirection.Equals(OrderByDirection, StringComparison.OrdinalIgnoreCase);
    }
}
