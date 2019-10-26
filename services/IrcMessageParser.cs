using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;

namespace TwitchBot.Services
{
    public class IrcMessageParser : ITwitchMessageSubject, IIrcMessageSubject
    {
        private readonly ILogger _logger;
        private IrcClient _client;

        private IList<ITwitchMessageObserver> _messageObservers = new List<ITwitchMessageObserver>();
        private IList<IIrcMessageObserver> _ircObservers = new List<IIrcMessageObserver>();

        public IrcMessageParser(ILogger<IrcMessageParser> logger, IrcClient client)
        {
            _logger = logger;
            _client = client;
        }

        public void Attach(ITwitchMessageObserver TwitchMessageObserver)
        {
            _messageObservers.Add(TwitchMessageObserver);
            _logger.LogInformation($"Registered {nameof(ITwitchMessageObserver)} {TwitchMessageObserver.GetName()}");
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
                    _logger.LogInformation($"Message observer updated {observer.GetName()}");
                }
            });
        }
        private async Task NotifyIrcObservers(IrcMessage Message)
        {
            await Task.Run(() => {

                foreach (var observer in _ircObservers)
                {
                    observer.Update(Message);
                    _logger.LogInformation($"IRC observer updated {observer.GetName()}");
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
                notifyTask = NotifyIrcObservers(ircMessage);
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

        public void Attach(IIrcMessageObserver IrcMessageObserver)
        {
            _ircObservers.Add(IrcMessageObserver);
            _logger.LogInformation($"Registered {nameof(IIrcMessageObserver)} {IrcMessageObserver.GetName()}");
        }

        public void Detach(IIrcMessageObserver IrcMessageObserver)
        {
            _ircObservers.Remove(IrcMessageObserver);
        }
    }
}