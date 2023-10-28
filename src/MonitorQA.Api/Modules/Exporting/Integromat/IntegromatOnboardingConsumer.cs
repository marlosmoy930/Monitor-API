using Microsoft.Extensions.Logging;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Utils;
using MonitorQA.Utils.Configurations;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.Exporting.Integromat
{
    public class IntegromatOnboardingConsumer : ICloudConsumer<IntegromatOnboardingMessage>
    {
        private readonly ConfigurationData _configurationData;
        private readonly ILogger<IntegromatOnboardingConsumer> _logger;

        public IntegromatOnboardingConsumer(
            ConfigurationData configurationData, 
            ILogger<IntegromatOnboardingConsumer> logger)
        {
            _configurationData = configurationData;
            _logger = logger;
        }

        public async Task ConsumeAsync(IntegromatOnboardingMessage message)
        {
            _logger.LogDebugObject(message);

            var isIntegromatEnabled = _configurationData.Integromat.IsEnabled;
            if (!isIntegromatEnabled) return;

            var json = JsonConvert.SerializeObject(message);

            var url = _configurationData.Integromat.OnboardingProgressWebhookUrl;

            _logger.LogDebug($"url: {url}");

            var webRequest = WebRequest.Create(new Uri(url));
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            
            var byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            webRequest.ContentLength = byteArray.Length;

            using (var dataStream = webRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using var response = await webRequest.GetResponseAsync();
        }
    }
}
