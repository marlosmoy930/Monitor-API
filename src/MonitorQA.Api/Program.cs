using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MonitorQA.Utils.Configurations;
using System;

namespace MonitorQA.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using var host = BuildWebHost(args);
                host.Run();

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Environment.Exit(1);
            }
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var host = WebHost
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                    ConfigurationUtil.GetConfigurationFromBuilder(builder, environmentName);
                })
                .UseSentry()
                .UseStartup<Startup>()
                .Build();

            return host;
        }
    }
}
