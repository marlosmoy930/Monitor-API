using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.UserGroups.Models
{
    public class UserGroupModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Guid> UserIds { get; set; }
    }
}
