using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorQA.Api.Modules.UserGroups.Models
{
    public class CreateUserGroupModel
    {
        public string Name { get; set; }

        public IEnumerable<Guid> UserIds { get; set; }

        public UserGroup CreateEntity(Guid compnayId)
        {
            var userGroup = new UserGroup
            {
                Name = Name,
                CompanyId = compnayId
            };
            userGroup.UserUserGroups = UserIds
                .Select(userId => new UserUserGroup
                {
                    UserId = userId,
                }).ToList();

            return userGroup;
        }
    }
}
