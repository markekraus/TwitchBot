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

        public ConsoleApplication(ILogger<ConsoleApplication> logger, IOptions<AppSettings> config, IrcClient client)
        {
            _logger = logger;
            _config = config;
            _client = client;
        }
        public async Task Run(){
            _logger.LogInformation($"Console Application Start {DateTime.UtcNow}");
            string message;
            do
            {
                message = await _client.ReadMessageAsync();
            } while (!string.IsNullOrEmpty(message));
            _logger.LogInformation($"Console Application End {DateTime.UtcNow}");
            Thread.Sleep(500);
        }
    }
}