using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchBot.Commands;
using TwitchBot.Interfaces;
using TwitchBot.Options;
using TwitchBot.Services;

namespace TwitchBot
{
    public class ConsoleApplication
    {
        private readonly ILogger _logger;
        private readonly IOptions<AppSettings> _config;
        private readonly IrcClient _client;
        private readonly IrcMessageParser _ircParser;
        private readonly TwitchCommandParser _commandParser;
        private readonly PingHandler _pingHandler;
        private List<ITwitchCommandObserver> commandObservers = new List<ITwitchCommandObserver>();

        public ConsoleApplication(
            ILogger<ConsoleApplication> logger,
            IOptions<AppSettings> config,
            IrcClient client,
            IrcMessageParser ircParser,
            TwitchCommandParser commandParser,
            PingHandler pingHandler,
            CatfactsCommand catfactCommand,
            IssLocationCommand iisLocationCommand,
            ExchangeRateCommand exchangeRateCommand,
            UrbanDictionaryCommand urbanDictionaryCommand,
            BrbCommand brbCommand,
            HiMarkCommand hiMarkCommand,
            SoundsCommand SoundsCommand,
            RollCommand RollCommand,
            TestCommand testCommand)

        {
            _logger = logger;
            _config = config;
            _client = client;
            _ircParser = ircParser;
            _commandParser = commandParser;
            _pingHandler = pingHandler;
            commandObservers.Add(testCommand);
            commandObservers.Add(iisLocationCommand);
            commandObservers.Add(exchangeRateCommand);
            commandObservers.Add(catfactCommand);
            commandObservers.Add(brbCommand);
            commandObservers.Add(urbanDictionaryCommand);
            commandObservers.Add(hiMarkCommand);
            commandObservers.Add(SoundsCommand);
            commandObservers.Add(RollCommand);
        }
        public async Task Run(){
            await Task.Run(() =>
            {
                _logger.LogInformation($"Console Application Start {DateTime.UtcNow}");
                while(true)
                {
                    //noop
                }
            });
        }
    }
}
