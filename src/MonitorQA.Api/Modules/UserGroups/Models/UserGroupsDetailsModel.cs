using System.Collections.Generic;

namespace MonitorQA.Api.Modules.UserGroups.Models
{
    public class UserGroupsDetailsModel
    {
        public int AvailableUsersCount { get; set; }

        public int UngroupedUsersCount { get; set; }

        public IEnumerable<UserGroupModel> Groups { get; set; }
    }
}
