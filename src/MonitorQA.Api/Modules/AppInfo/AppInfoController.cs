using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Data;
using MonitorQA.Utils.Configurations;

namespace MonitorQA.Api.Modules.AppInfo
{
    [Route("[controller]")]
    public class AppInfoController: AuthorizedController
    {
        private readonly ConfigurationData _configurationData;

        public AppInfoController(
            SiteContext context,
            ConfigurationData configurationData) : base(context)
        {
            this._configurationData = configurationData;
        }

        [HttpGet]
        [Route("min-mobile-version")]
        public async Task<IActionResult> GetMinMobileVersion()
        {
            var mobile = _configurationData.App.Mobile;
            return Ok(mobile);
        }

    }
}
