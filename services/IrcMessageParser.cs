using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;

namespace TwitchBot.Services
{
    public class IrcMessageParser : ITwitchMessageSubject, IIrcMessageSubject, IIrcClientObserver
    {
        private readonly ILogger _logger;
        private IIrcClient _client;

        private IList<ITwitchMessageObserver> _messageObservers = new List<ITwitchMessageObserver>();
        private IList<IIrcMessageObserver> _ircObservers = new List<IIrcMessageObserver>();

        public IrcMessageParser(ILogger<IrcMessageParser> logger, IIrcClient client)
        {
            _logger = logger;
            _client = client;
            _client.Attach(this);
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

        public async Task Update(string Message)
        {
            await Task.Run(() =>
            {
                if (String.IsNullOrWhiteSpace(Message))
                {
                    return;
                }
                IrcMessage ircMessage;
                _logger.LogInformation($"Parser: {Message}");
                try
                {
                    ircMessage = new IrcMessage(Message);
                    _logger.LogInformation($"Parsed IRC Message {ircMessage}");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Unable to parse IRC message", Message);
                    return;
                }
                var notifyTask = NotifyIrcObservers(ircMessage);
                if (ircMessage.Action == TwitchMessage.AllowedAction)
                {
                    TwitchMessage twitchMessage;
                    try
                    {
                        twitchMessage = new TwitchMessage(ircMessage);
                        _logger.LogInformation($"Parsed Twitch Message {twitchMessage}");
                        notifyTask = NotifyMessageObservers(twitchMessage);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Unable to parse Twitch message", ircMessage);
                    }
                }
            });
            
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

        public Task Reconnect()
        {
            return Task.Run(() => {});
        }
    }
}