using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Companies.Models
{
    public class UpdateCompanyRequest
    {
        public string? Name { get; set; }
        
        public CompanySize? CompanySize { get; set; }
        
        public CompanyIndustry? Industry { get; set; }
        
        public CompanyUsagePurpose? UsagePurpose { get; set; }
        
        public string? UsagePurposeObjectName { get; set; }

        public void UpdateCompany(Company company)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                company.Name = Name;
            }
            company.CompanySize = CompanySize ?? company.CompanySize;
            company.Industry = Industry ?? company.Industry;
            company.UsagePurpose = UsagePurpose ?? company.UsagePurpose;

            if (!string.IsNullOrEmpty(UsagePurposeObjectName))
            {
                company.CustomAuditObjectName = UsagePurposeObjectName;
            }
        }
    }

}
