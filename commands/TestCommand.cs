using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Commands
{
    public class TestCommand : ITwitchCommandObserver
    {
        private ILogger<TestCommand> _logger;
        private ITwitchCommandSubject _subject;
        private IrcClient _client;
        private Task runner;

        public const string Command = "!test";

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public TestCommand(ILogger<TestCommand> logger, ITwitchCommandSubject subject, IrcClient client)
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
                    if (command.HasParameters && command.Parameters[0] == "123")
                    {
                        task = _client.SendPublicChatMessageAsync($"Valid !test Received from @{command.Message.TwitchUser.DisplayName}");
                    }
                    else
                    {
                        task = _client.SendPublicChatMessageAsync($"Invalid !test Received from @{command.Message.TwitchUser.DisplayName}");
                    }
                }
            });
        }

        public string GetCommand()
        {
            return Command;
        }
    }
}