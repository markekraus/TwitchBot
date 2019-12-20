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
using TwitchBot.Models;
using System.Collections.Generic;

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
            services.Configure<IrcSettings>( _option =>
            {
                _option.DefaultChannel = configuration.GetValue<string>("BotOwner").ToLower();
                _option.UserName = configuration.GetValue<string>("BotUserName").ToLower();
                _option.Password = configuration.GetValue<string>("BotPassword",string.Empty).ToLower();
                _option.Port = 6697;
                _option.HostName = "irc.chat.twitch.tv";
                _option.EnableTls = true;

            });
            services.AddLogging(logging => 
            {
                logging
                    .ClearProviders()
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole( c =>
                    {
                        c.TimestampFormat = "[HH:mm:ss] ";
                    });
            });
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IrcClient>();
            services.AddSingleton<IIrcClient>(x => x.GetRequiredService<IrcClient>());
            services.Configure<TwitchIrcClientAdapterSettings>( _options => 
            {
               _options.BotUser = new TwitchUser()
                {
                    UserName = configuration.GetValue<string>("BotUserName").ToLower()
                };
                _options.Channels = configuration.GetSection("Channels").Get<List<string>>();
            });
            services.AddSingleton<TwitchIrcClientAdapter>();
            services.AddSingleton<ITwitchIrcClientAdapter>(x => x.GetRequiredService<TwitchIrcClientAdapter>());
            services.AddSingleton<IrcMessageParser>();
            services.AddSingleton<ITwitchMessageSubject>(x => x.GetRequiredService<IrcMessageParser>());
            services.AddSingleton<IIrcMessageSubject>(x => x.GetRequiredService<IrcMessageParser>());
            services.AddSingleton<TwitchCommandParser>();
            services.AddSingleton<ITwitchCommandSubject>(x => x.GetRequiredService<TwitchCommandParser>());
            services.AddSingleton<TestCommand>();
            services.AddSingleton<CatfactsCommand>();
            services.AddSingleton<IssLocationCommand>();
            services.AddSingleton<ExchangeRateCommand>();
            services.AddSingleton<UrbanDictionaryCommand>();
            services.AddSingleton<BrbCommand>();
            services.AddSingleton<HiMarkCommand>();
            services.AddSingleton<RollCommand>();
            services.Configure<SoundsCommandSettings>(_option =>
            {
                _option.Sounds = configuration.GetSection("Sounds").Get<List<Sound>>();
                _option.SoundsChannel = configuration.GetValue<string>("BotOwner");
            });
            services.AddSingleton<SoundsCommand>();
            services.AddSingleton<PingHandler>();
            services.AddTransient<ConsoleApplication>();
            return services;
        }
    }
}
