using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Commands
{
    public class HiMarkCommand : ITwitchCommandObserver
    {
        private ILogger<HiMarkCommand> _logger;
        private ITwitchCommandSubject _subject;
        private ITwitchIrcClientAdapter _client;
        private Task runner;

        public const string PrimaryCommand = "!himark";

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public HiMarkCommand(
            ILogger<HiMarkCommand> logger, 
            ITwitchCommandSubject subject, 
            ITwitchIrcClientAdapter client)
        {
            _logger = logger;
            _subject = subject;
            _client = client;

            subject.Attach(this);
            runner = Run();
        }

        public async Task Update(TwitchChatCommand Command)
        {
            await Task.Run(() => { queue.Add(Command); });
        }

        private Task Run()
        {
            return Task.Run(()=>
            {
                Task task;
                string message;
                foreach (var command in queue.GetConsumingEnumerable())
                {
                    _logger.LogInformation($"Processing {command.Command}...");
                    message = "Oh hai Mark!";
                    task = _client.SendPublicChatMessageAsync(Message: message, Channel: command.Message.IrcChannel);
                }
            });
        }

        public string GetPrimaryCommand()
        {
            return PrimaryCommand;
        }

        public bool IsCommandSupported(string Command)
        {
            return string.Equals(Command, PrimaryCommand, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
