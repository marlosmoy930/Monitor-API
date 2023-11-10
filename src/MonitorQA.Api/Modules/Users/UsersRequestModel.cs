using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Linq;

namespace MonitorQA.Api.Modules.Users
{
    public class UsersRequestModel : ListRequestModel
    {
        const string orderByEmailFieldName = "email";
        const string orderByRoleFieldName = "roleName";

        public string Status { get; set; } = "all";

        public Guid? UserGroupId { get; set; }

        public UserGroupFilterType GroupFilterType { get; set; }

        public IQueryable<User> Filter(IQueryable<User> query, Guid companyId)
        {
            query = query
                .Where(u => u.CompanyId == companyId && !u.IsDeleted);

            switch (Status)
            {
                case "active":
                    query = query.Where(u => u.IsActive);
                    break;
                case "suspended":
                    query = query.Where(u => !u.IsActive);
                    break;
            }

            if (!string.IsNullOrEmpty(Search))
                query = query.Where(u => u.Name.Contains(Search) || u.Role.Name.Contains(Search));

            if (GroupFilterType == UserGroupFilterType.All)
            {
            }
            else if (GroupFilterType == UserGroupFilterType.ByGroup)
            {
                query = query.Where(u => u.UserUserGroups.Any(ug => ug.UserGroupId == UserGroupId.Value));
            }
            else if (GroupFilterType == UserGroupFilterType.Ungrouped)
            {
                query = query.Where(u => !u.UserUserGroups.Any());
            }
            else
            {
                throw new NotImplementedException($"{nameof(GroupFilterType)} = {GroupFilterType}");
            }

            return query;
        }

        public IOrderedQueryable<User> ApplyOrder(IQueryable<User> query)
        {
            if (OrderBy == OrderByNameFieldName)
            {
                return IsAscendingOrderDirection
                    ? query.OrderBy(s => s.Name)
                    : query.OrderByDescending(s => s.Name);
            }

            if (OrderBy == orderByEmailFieldName)
            {
                return IsAscendingOrderDirection
                    ? query.OrderBy(s => s.Email)
                    : query.OrderByDescending(s => s.Email);
            }

            if (OrderBy == orderByRoleFieldName)
            {
                return IsAscendingOrderDirection
                    ? query.OrderBy(s => s.Role.Name)
                    : query.OrderByDescending(s => s.Role.Name);
            }


            throw new NotImplementedException($"{nameof(OrderBy)} is {OrderBy}");
        }
    }
}