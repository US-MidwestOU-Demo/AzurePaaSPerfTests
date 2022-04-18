using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using CosmosOperations.DataAccess;
using CosmosOperations.Models;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, configuration) =>
    {
        IHostEnvironment env = hostingContext.HostingEnvironment;
            
        configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
    })
    .ConfigureServices((services) =>
    {
        services.AddHostedService<CosmosOperations.TestDriverConsole>();
        services.AddLogging();
        services.AddSingleton<DatabaseClientSingleton>();
        services.AddScoped<DatabaseOperations<Transaction>>();
    })
    .Build();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hosted Console App Started");

await host.RunAsync();