using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MonitorQA.Api.Modules.Users
{
    public class UserDetail
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public IdNamePairModel<Guid> Role { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<Guid> UserGroupIds { get; set; }

        public IEnumerable<TagModel> Tags { get; set; }

        public static Expression<Func<User, UserDetail>> GetSelectExpression()
        {
            return u => new UserDetail
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = new IdNamePairModel<Guid> { Id = u.Role.Id, Name = u.Role.Name },
                IsActive = u.IsActive,
                UserGroupIds = u.UserUserGroups.Select(ug => ug.UserGroupId),
                Tags = u.UserTags
                    .OrderBy(tags => tags.Tag.Name)
                    .Select(tags => new TagModel
                    {
                        Id = tags.Tag.Id,
                        Name = tags.Tag.Name,
                        Color = tags.Tag.Color
                    })
            };
        }
    }
}