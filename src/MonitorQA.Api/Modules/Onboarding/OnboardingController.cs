using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Modules.Exporting.Integromat;
using MonitorQA.Api.Modules.Onboarding.Models;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Data;
using MonitorQA.Utils.Configurations;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.Onboarding
{
    [Route("onboarding")]
    [ApiController]
    public class OnboardingController : AuthorizedController
    {
        private readonly SiteContext _context;
        private readonly ConfigurationData _configurationData;
        private readonly CloudMessagePublisher _publisher;

        public OnboardingController(
            SiteContext context,
            ConfigurationData configurationData,
            CloudMessagePublisher publisher) : base(context)
        {
            this._context = context;
            _configurationData = configurationData;
            _publisher = publisher;
        }

        [HttpGet]
        [Route("progress")]
        public async Task<IActionResult> GetProgress()
        {
            var companyId = CurrentUser.CompanyId;
            var protressInfo = await _context.Companies
                .AsNoTracking()
                .Where(x => x.Id == companyId)
                .Select(OnboardingProgress.GetSelectExpression())
                .SingleAsync();

            return Ok(protressInfo);
        }

        [HttpPut]
        [Route("progress")]
        public async Task<IActionResult> UpdateProgress([FromBody] OnboardingProgress progress)
        {
            var companyId = CurrentUser.CompanyId;
            var company = await _context.Companies
                .AsQueryable()
                .SingleAsync(x => x.Id == companyId);

            progress.UpdateCompany(company);
            await _context.SaveChangesAsync();

            var isIntegromatEnabled = _configurationData.Integromat.IsEnabled;
            if (isIntegromatEnabled)
            {
                var isProduction = _configurationData.App.IsProduction;
                var message = IntegromatOnboardingMessage.Create(isProduction, company, progress.UserCookie , CurrentUser);
                await _publisher.Publish(message);
            }

            return Ok();
        }
    }
}
