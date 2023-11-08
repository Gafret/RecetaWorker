using System.Data;
using Npgsql;
using RecipeFetcherService;
using RecipeFetcherService.Scraper;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<DbOptions>(
    builder.Configuration.GetSection(DbOptions.ConnectionStrings));
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();