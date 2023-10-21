using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MonitorQA.Api.Middleware
{
    public class LogMiddleware
    {
        private static readonly string[] methodsWithBody = new string[] { "POST", "PUT" };
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);
            await _next(context);
        }

        private async Task LogRequest(HttpContext ctx)
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var request = ctx.Request;

            if (request.Headers.TryGetValue("user-agent", out var userAgent))
            {
                var userAgentStr = userAgent
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray()[0]
                    .Split("/")[0];

                if (userAgentStr == "Dart")
                {
                    var method = request.Method;

                    var body = "";
                    if (methodsWithBody.Contains(method))
                    {
                        request.EnableBuffering();
                        using (var reader = new StreamReader(
                            request.Body, 
                            encoding: Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: false,
                            leaveOpen: true))
                        {
                            body = await reader.ReadToEndAsync();
                            request.Body.Position = 0;
                        }
                        body = "\t" + body;
                    }

                    var remoteId = ctx.Connection.RemoteIpAddress;
                    var message = $"{request.Method}\t{remoteId}\t{request.Path}{request.QueryString}{body}";
                    _logger.LogDebug(message);
                }
            }

        }

    }
}
