using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Azure.Identity;
using Myfunction.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        var serviceUri = new Uri($"https://stfuncmyfunctionapp1.table.core.windows.net");

        services.AddSingleton(serviceProvider =>
        {
            var tableClient = new TableClient(serviceUri, "Products", new DefaultAzureCredential());
            tableClient.CreateIfNotExists();
            return tableClient;
        });

        services.AddScoped<IProductService, ProductService>();
    })
    .Build();

host.Run();