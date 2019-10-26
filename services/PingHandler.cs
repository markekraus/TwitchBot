using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;

namespace TwitchBot.Services
{
    public class PingHandler : IIrcMessageObserver
    {
        private IIrcMessageSubject _subject;
        private ILogger<PingHandler> _logger;
        private BlockingCollection<IrcMessage> queue;

        private IrcClient _client;

        private Task runner;
        public PingHandler(
            ILogger<PingHandler> logger, 
            IIrcMessageSubject subject,
            IrcClient client)
        {
            _logger = logger;
            _subject = subject;
            _client = client;
            queue = new BlockingCollection<IrcMessage>(new ConcurrentQueue<IrcMessage>());
            subject.Attach(this);
            runner = Run();
        }

        public string GetName()
        {
            return nameof(PingHandler);
        }

        public async Task Update(IrcMessage Message)
        {
            await Task.Run(() => { queue.Add(Message); });
        }

        private Task Run()
        {
            return Task.Run(() =>
            {
                _logger.LogInformation($"{nameof(PingHandler)} running.");
                Task task;
                string sendmessage;
                foreach (var message in queue.GetConsumingEnumerable())
                {
                    _logger.LogInformation($"Dequeuing message {message} action '{message.Action}'");
                    if (message.Action == "PING")
                    {
                        sendmessage = $"PONG {message.Source}";
                        _logger.LogInformation($"Sending message: {sendmessage}");
                        task = _client.SendIrcMessageAsync(sendmessage);
                    }
                }
            });
        }
    }
}