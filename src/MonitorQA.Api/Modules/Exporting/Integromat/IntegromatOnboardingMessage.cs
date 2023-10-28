using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.Exporting.Integromat
{
    public class IntegromatOnboardingMessage : IntegromatCloudMessageBase
    {
        public bool IsProduction { get; set; }

        public string? UserCookie { get; set; }

        public DateTime? CompanyCreatedAt { get; set; }

        public string? CompanyName { get; set; }

        public CompanyIndustry? CompanyIndustry { get; set; }

        public CompanySize? CompanySize { get; set; }

        public CompanyUsagePurpose? UsagePurpose { get; set; }

        public string? UserName { get; set; }

        public string? UserEmail { get; set; }

        public string? UserRole { get; set; }

        public bool SetupComplete => SetupCompanyOnboardingStepComplete.GetValueOrDefault()
            && AddAuditObjectOnboardingStepComplete.GetValueOrDefault()
            && AddUsersOnboardingStepComplete.GetValueOrDefault()
            && CreateTemplatePopoversStepComplete.GetValueOrDefault();

        public bool? SetupCompanyOnboardingStepComplete { get; set; }

        public bool? AddUsersOnboardingStepComplete { get; set; }

        public bool? AddAuditObjectOnboardingStepComplete { get; set; }

        public bool? CreateTemplateOnboardingStepComplete { get; set; }

        public bool? ScheduleAuditOnboardingStepComplete { get; set; }

        public bool? AddUsersPopoversStepComplete { get; set; }

        public bool? AddAuditObjectPopoversStepComplete { get; set; }

        public bool? ManageTemplatesPopoversStepComplete { get; set; }

        public bool? CreateTemplatePopoversStepComplete { get; set; }

        public bool? ScheduleAuditPopoversStepComplete { get; set; }

        public static IntegromatOnboardingMessage Create(
            bool isProduction,
            Company company,
            string? userCookie,
            User user)
        {
            var message = new IntegromatOnboardingMessage
            {
                IsProduction = isProduction,

                CompanyCreatedAt = company.CreatedAt,
                CompanyName = company.Name,
                CompanyIndustry = company.Industry,
                CompanySize = company.CompanySize,
                UsagePurpose = company.UsagePurpose,

                UserCookie = userCookie,
                UserName = user.Name,
                UserEmail = user.Email,
                UserRole = user.Role.Name,

                SetupCompanyOnboardingStepComplete = company.IsSetupCompanyOnboardingStepComplete,
                AddUsersOnboardingStepComplete = company.IsAddUsersOnboardingStepComplete,
                AddAuditObjectOnboardingStepComplete = company.IsAddAuditObjectOnboardingStepComplete,
                CreateTemplateOnboardingStepComplete = company.IsCreateTemplateOnboardingStepComplete,
                ScheduleAuditOnboardingStepComplete = company.IsScheduleAuditOnboardingStepComplete,
                AddUsersPopoversStepComplete = company.IsAddUsersPopoversStepComplete,
                AddAuditObjectPopoversStepComplete = company.IsAddAuditObjectPopoversStepComplete,
                ManageTemplatesPopoversStepComplete = company.IsManageTemplatesPopoversStepComplete,
                CreateTemplatePopoversStepComplete = company.IsCreateTemplatePopoversStepComplete,
                ScheduleAuditPopoversStepComplete = company.IsScheduleAuditPopoversStepComplete,
            };
            return message;
        }
    }
}
