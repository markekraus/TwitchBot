using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TwitchBot.Services
{
    public class IrcMessageParser
    {
        private readonly ILogger _logger;
        private IrcClient _client;

        public IrcMessageParser(ILogger<IrcMessageParser> logger, IrcClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task Run()
        {
            _logger.LogInformation($"{nameof(IrcMessageParser.Run)}() start {DateTime.UtcNow}");
            string rawMessage;
            var cts = new CancellationTokenSource();
            do
            {
                rawMessage = await _client.ReadMessageAsync(cts.Token);
                _logger.LogInformation($"Parser: {rawMessage}");
            } while (!string.IsNullOrEmpty(rawMessage));
        }
    }
}