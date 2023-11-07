using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Linq;

namespace MonitorQA.Api.Modules.Scores.Models
{
    public class ScoreListRequestModel: ListRequestModel
    {
        public IQueryable<ScoreSystem> Filter(IQueryable<ScoreSystem> query, User user)
        {
            query = query
                .Where(ScoreSystem.Predicates.NotDeleted())
                .Where(ScoreSystem.Predicates.SameCompany(user));

            if (!string.IsNullOrEmpty(Search))
            {
                var searchUpper = Search.ToUpperInvariant();
                query = query.Where(s => s.Name.ToUpper().Contains(searchUpper));
            }

            return query;
        }

        public IOrderedQueryable<ScoreSystem> ApplyOrder(IQueryable<ScoreSystem> query)
        {
            if (OrderBy == OrderByNameFieldName)
            {
                return IsAscendingOrderDirection
                    ? query.OrderBy(s => s.Name)
                    : query.OrderByDescending(s => s.Name);
            }

            throw new NotImplementedException($"{nameof(OrderBy)} is {OrderBy}");
        }
    }
}