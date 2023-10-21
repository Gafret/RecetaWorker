using RecipeFetcherService;
using RecipeFetcherService.Scraper;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .Build();



host.Run();