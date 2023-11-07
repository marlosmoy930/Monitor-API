namespace MonitorQA.Api.Modules.Roles
{
    public class RoleModel
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public bool? CanDoAudits { get; set; }

        public bool? CanAssignAudits { get; set; }

        public bool? CanManageAudits { get; set; }

        public bool? CanScheduleAudits { get; set; }

        public bool? CanDoCorrectiveActions { get; set; }

        public bool? CanApproveCorrectiveActions { get; set; }

        public bool? CanAssignCorrectiveActions { get; set; }

        public bool? CanViewAuditsResults { get; set; }

        public bool? CanManageUsers { get; set; }

        public bool? CanManageAuditObjects { get; set; }

        public bool? CanManageTemplates { get; set; }

        public bool? CanManageTags { get; set; }

        public bool? CanManageRoles { get; set; }

        public bool? CanManageScoreSystems { get; set; }
    }
}