using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchBot.Options;
using TwitchBot.Services;

namespace TwitchBot
{
    public class ConsoleApplication
    {
        private readonly ILogger _logger;
        private readonly IOptions<AppSettings> _config;
        private readonly IrcClient _client;
        private readonly IrcMessageParser _parser;

        public ConsoleApplication(
            ILogger<ConsoleApplication> logger,
            IOptions<AppSettings> config,
            IrcClient client,
            IrcMessageParser parser)
        {
            _logger = logger;
            _config = config;
            _client = client;
            _parser = parser;
        }
        public async Task Run(){
            _logger.LogInformation($"Console Application Start {DateTime.UtcNow}");
            await _parser.Run();
            _logger.LogInformation($"Console Application End {DateTime.UtcNow}");
            Thread.Sleep(500);
        }
    }
}