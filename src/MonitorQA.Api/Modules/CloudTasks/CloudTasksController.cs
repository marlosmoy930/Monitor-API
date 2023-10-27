using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MonitorQA.Cloud.Messaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.CloudTasks
{
    [Route(CloudMessagePublisher.CloudTaskRelativeUrl)]
    [ApiController]
    public class CloudTasksController : ControllerBase
    {
        private readonly CloudMessageHandler _handler;
        private readonly ILogger<CloudTasksController> _logger;

        public CloudTasksController(
            CloudMessageHandler handler,
            ILogger<CloudTasksController> logger)
        {
            this._handler = handler;
            this._logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromHeader(Name = CloudMessagePublisher.MessageTypeNameKey)] string messageTypeFullName)
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            await _handler.Handle(json, messageTypeFullName, _logger);
            return Ok();
        }
    }
}
