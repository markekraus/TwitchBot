using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.IO;
using TwitchBot.Options;
using TwitchBot.Services;
using System.Threading.Tasks;
using TwitchBot.Interfaces;
using TwitchBot.Commands;
using System.Net.Http;

namespace TwitchBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            var services = ConfigureServices();   
            var serviceProvider = services.BuildServiceProvider();

            await serviceProvider.GetService<ConsoleApplication>().Run();
        }

        private static IServiceCollection ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables($"{nameof(TwitchBot)}_")
                .Build();
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<AppSettings>(configuration);
            services.Configure<IrcSettings>( _option => {
                _option.Channel = configuration.GetValue<string>("StreamerChannel").ToLower();
                _option.UserName = configuration.GetValue<string>("BotUserName").ToLower();
                _option.Password = configuration.GetValue<string>("BotPassword",string.Empty).ToLower();
            });
            services.AddLogging(logging => {
                logging
                    .ClearProviders()
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IrcClient>();
            services.AddSingleton<IrcMessageParser>();
            services.AddSingleton<ITwitchMessageSubject>(x => x.GetRequiredService<IrcMessageParser>());
            services.AddSingleton<IIrcMessageSubject>(x => x.GetRequiredService<IrcMessageParser>());
            services.AddSingleton<TwitchCommandParser>();
            services.AddSingleton<ITwitchCommandSubject>(x => x.GetRequiredService<TwitchCommandParser>());
            services.AddSingleton<TestCommand>();
            services.AddSingleton<CatfactsCommand>();
            services.AddSingleton<IssLocationCommand>();
            services.AddSingleton<ExchangeRateCommand>();
            services.AddSingleton<PingHandler>();
            services.AddTransient<ConsoleApplication>();
            return services;
        }
    }
}
