using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;

namespace TwitchBot.Services
{
    public class IrcMessageParser : ITwitchMessageSubject
    {
        private readonly ILogger _logger;
        private IrcClient _client;

        private IList<ITwitchMessageObserver> _messageObservers = new List<ITwitchMessageObserver>();

        public IrcMessageParser(ILogger<IrcMessageParser> logger, IrcClient client)
        {
            _logger = logger;
            _client = client;
        }

        public void Attach(ITwitchMessageObserver TwitchMessageObserver)
        {
            _messageObservers.Add(TwitchMessageObserver);
        }

        public void Detach(ITwitchMessageObserver TwitchMessageObserver)
        {
            _messageObservers.Remove(TwitchMessageObserver);
        }

        private async Task NotifyMessageObservers(TwitchMessage Message)
        {
            await Task.Run(() => {

                foreach (var observer in _messageObservers)
                {
                    observer.Update(Message);
                }
            });
        }

        public async Task Run()
        {
            _logger.LogInformation($"{nameof(IrcMessageParser.Run)}() start {DateTime.UtcNow}");
            string rawMessage;
            IrcMessage ircMessage;
            TwitchMessage twitchMessage;
            Task notifyTask;
            var cts = new CancellationTokenSource();
            do
            {
                rawMessage = null;
                ircMessage = null;
                twitchMessage = null;
                rawMessage = await _client.ReadMessageAsync(cts.Token);
                _logger.LogInformation($"Parser: {rawMessage}");
                try
                {
                    ircMessage = new IrcMessage(rawMessage);
                    _logger.LogInformation($"Parsed IRC Message {ircMessage}");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Unable to parse IRC message", rawMessage);
                    continue;
                }
                if (ircMessage.Action == TwitchMessage.AllowedAction)
                {
                    try
                    {
                        twitchMessage = new TwitchMessage(ircMessage);
                        _logger.LogInformation($"Parsed Twitch Message {twitchMessage}");
                        notifyTask = NotifyMessageObservers(twitchMessage);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Unable to parse Twitch message", twitchMessage);
                    }
                }
            } while (!string.IsNullOrEmpty(rawMessage));
        }
    }
}