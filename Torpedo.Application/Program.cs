using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Torpedo.Bot;
using Torpedo.Converters.Video;
using Torpedo.Infrastructure;

namespace Torpedo.Application
{
    internal class Program
    {
        static IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder().ConfigureServices((hostContext, services) =>
            {
                var config = new Settings();
                Configuration.Bind(config);
                services.AddSingleton(config);
                services.AddSingleton<XabeConverter>();
                services.AddSingleton<TelegramBot>();
                services.AddSingleton<DiscordBot>();

            });

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Torpedo Club Features © 2022 Vitasya Tavern");

            var host = CreateHostBuilder(args).Build();
            host.Services.GetRequiredService<DiscordBot>();

            await host.RunAsync();
        }
    }
}