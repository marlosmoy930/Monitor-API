
using System.Collections;
using System.Collections.Generic;

namespace MonitorQA.Api.Infrastructure.Models
{
    public interface IPagingRequestModel
    {
        int PageNumber { get; set; }

        int PageSize { get; set; }
    }

    public class PagingModel : IPagingRequestModel
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int? TotalCount { get; set; }
    }

    public class ListPageResult
    {
        public IEnumerable Data { get; }

        public PagingModel Meta { get; }

        public ListPageResult(IEnumerable data, int count, ListRequestModel model)
        {
            Data = data;
            Meta = new PagingModel
            {
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                TotalCount = count
            };
        }
    }

    public class ListPageResult<T>
    {
        public IEnumerable<T> Data { get; private set; }

        public PagingModel Meta { get; private set; }

        public static ListPageResult<T> Create<T>(IEnumerable<T> data, int count, ListRequestModel model)
        {
            return new ListPageResult<T>
            {
                Data = data,
                Meta = new PagingModel
                {
                    PageSize = model.PageSize,
                    PageNumber = model.PageNumber,
                    TotalCount = count
                }
            };
        }
    }

    public static class PagingModelExtensions
    {
        public static int GetSkipNumber(this IPagingRequestModel model)
        {
            return (model.PageNumber - 1) * model.PageSize;
        }
    }
}