using MonitorQA.Data.Entities;
using System;
using System.Linq.Expressions;

namespace MonitorQA.Api.Modules.Onboarding.Models
{
    public class OnboardingProgress
    {
        public string? UserCookie { get; set; }
        
        public bool? SetupCompany { get; set; }

        public bool? AddUsers { get; set; }

        public bool? AddAuditObject { get; set; }

        public bool? CreateTemplate { get; set; }

        public bool? ScheduleAudit { get; set; }

        public bool? AddUsersPopovers { get; set; }

        public bool? AddAuditObjectPopovers { get; set; }

        public bool? ManageTemplatesPopovers { get; set; }

        public bool? CreateTemplatePopovers { get; set; }

        public bool? ScheduleAuditPopovers { get; set; }


        public static Expression<Func<Company, OnboardingProgress>> GetSelectExpression()
        {
            return x => new OnboardingProgress
            {
                SetupCompany = x.IsSetupCompanyOnboardingStepComplete,
                AddUsers = x.IsAddUsersOnboardingStepComplete,
                AddAuditObject = x.IsAddAuditObjectOnboardingStepComplete,
                CreateTemplate = x.IsCreateTemplateOnboardingStepComplete,
                ScheduleAudit = x.IsScheduleAuditOnboardingStepComplete,
                AddUsersPopovers = x.IsAddUsersPopoversStepComplete,
                AddAuditObjectPopovers = x.IsAddAuditObjectPopoversStepComplete,
                ManageTemplatesPopovers = x.IsManageTemplatesPopoversStepComplete,
                CreateTemplatePopovers = x.IsCreateTemplatePopoversStepComplete,
                ScheduleAuditPopovers = x.IsScheduleAuditPopoversStepComplete,
            };
        }

        public void UpdateCompany(Company company)
        {
            company.IsSetupCompanyOnboardingStepComplete = SetupCompany ?? company.IsSetupCompanyOnboardingStepComplete;
            company.IsAddUsersOnboardingStepComplete = AddUsers ?? company.IsAddUsersOnboardingStepComplete;
            company.IsAddAuditObjectOnboardingStepComplete = AddAuditObject ?? company.IsAddAuditObjectOnboardingStepComplete;
            company.IsCreateTemplateOnboardingStepComplete = CreateTemplate ?? company.IsCreateTemplateOnboardingStepComplete;
            company.IsScheduleAuditOnboardingStepComplete = ScheduleAudit ?? company.IsScheduleAuditOnboardingStepComplete;
            company.IsAddUsersPopoversStepComplete = AddUsersPopovers ?? company.IsAddUsersPopoversStepComplete;
            company.IsAddAuditObjectPopoversStepComplete = AddAuditObjectPopovers ?? company.IsAddAuditObjectPopoversStepComplete;
            company.IsManageTemplatesPopoversStepComplete = ManageTemplatesPopovers ?? company.IsManageTemplatesPopoversStepComplete;
            company.IsCreateTemplatePopoversStepComplete = CreateTemplatePopovers ?? company.IsCreateTemplatePopoversStepComplete;
            company.IsScheduleAuditPopoversStepComplete = ScheduleAuditPopovers ?? company.IsScheduleAuditPopoversStepComplete;
        }
    }
}
