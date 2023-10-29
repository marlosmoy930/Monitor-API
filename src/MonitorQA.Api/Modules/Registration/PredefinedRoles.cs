using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.Registration
{
    public static class PredefinedRoles
    {
        public static Role[] GetRoles(Company company)
        {
            var roles = new[]
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Company = company,
                    Name = Role.AdminRoleName,
                    Description = "All permissions",
                    IsDefault = true,

                    CanDoAudits = true,
                    CanAssignAudits = true,
                    CanScheduleAudits = true,
                    CanManageAudits = true,
                    CanViewAuditsResults = true,

                    CanDoCorrectiveActions = true,
                    CanApproveCorrectiveActions = true,
                    CanAssignCorrectiveActions = true,

                    CanManageAuditObjects = true,
                    CanManageTemplates = true,
                    CanManageUsers = true,
                    CanManageRoles = true,
                    CanManageTags = true,

                    CanManageScoreSystems = true,
                },
                //new Role
                //{
                //    Id = Guid.NewGuid(),
                //    Company = company,
                //    Name = AdminRoleName,
                //    Description = "Create, edit and manage audits",
                //    IsDefault = true,

                //    CanDoAudits = true,
                //    CanAssignAudits = true,
                //    CanScheduleAudits = true,
                //    CanManageAudits = true,
                //    CanViewAuditsResults = true,

                //    CanDoCorrectiveActions = true,
                //    CanApproveCorrectiveActions = true,
                //    CanAssignCorrectiveActions = true,

                //    CanManageAuditObjects = true,
                //    CanManageTemplates = true,

                //    CanManageScoreSystems = true,
                //},
                new Role
                {
                    Id = Guid.NewGuid(),
                    Company = company,
                    Name = Role.AuditorRoleName,
                    Description = "All tasks related to performing audits and corrective actions",
                    IsDefault = true,

                    CanDoAudits = true,
                    CanAssignAudits = true,
                    CanViewAuditsResults = true,

                    CanDoCorrectiveActions = true,
                    CanApproveCorrectiveActions = true,
                    CanAssignCorrectiveActions = true,

                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Company = company,
                    Name = Role.ObserverRoleName,
                    Description = "View audit results, see corrective actions",
                    IsDefault = true,

                    CanViewAuditsResults = true,
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Company = company,
                    Name = Role.AuditeeRoleName,
                    Description = "View audit results and do corrective actions",
                    IsDefault = true,

                    CanViewAuditsResults = true,
                    CanDoCorrectiveActions = true,
                }
            };

            return roles;
        }
    }
}
