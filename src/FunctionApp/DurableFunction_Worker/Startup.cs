using Azure.Identity;
using DurableFunction_Worker.Models;
using DurableFunction_Worker.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(DurableFunction_Worker.Startup))]
namespace DurableFunction_Worker
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var context = builder.GetContext();
            var services = builder.Services;

            services.Configure<MySettings>(context.Configuration.GetSection("Function"));
            services.AddTransient<IDiceService, DiceService>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();


            if (context.EnvironmentName != "Development")
            {
                var config = builder.ConfigurationBuilder.Build();
                builder.ConfigurationBuilder
                    .AddAzureKeyVault(new Uri(config["Function:KeyVaultUrl"]), new DefaultAzureCredential());
            }
        }
    }
}
