using Microsoft.Extensions.DependencyInjection;
using Presentation;
using SpatialiteTest.Extentions;

class Program
{
     static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<App>().Run();
    }
}
