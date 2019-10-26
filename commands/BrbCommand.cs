using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Commands
{
    public class BrbCommand : ITwitchCommandObserver
    {
        private ILogger<BrbCommand> _logger;
        private ITwitchCommandSubject _subject;
        private IrcClient _client;
        private Task runner;

        public const string PrimaryCommand = "!brb";

        private Regex CommandRegex = new Regex("[!]{0,1}brb", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public BrbCommand(ILogger<BrbCommand> logger, ITwitchCommandSubject subject, IrcClient client)
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
                foreach (var command in queue.GetConsumingEnumerable())
                {
                    _logger.LogInformation($"Processing {command.Command}...");
                    task = _client.SendPublicChatMessageAsync($"We'll see you soon enough, @{command.Message.TwitchUser.DisplayName}!");
                }
            });
        }

        public string GetPrimaryCommand()
        {
            return PrimaryCommand;
        }

        public bool IsCommandSupported(string Command)
        {
            return CommandRegex.IsMatch(Command);
        }
    }
}