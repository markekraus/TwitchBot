using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;

namespace TwitchBot.Services
{
    public class TwitchCommandParser : ITwitchMessageObserver, ITwitchCommandSubject
    {
        private ITwitchMessageSubject _subject;
        private ILogger<TwitchCommandParser> _logger;
        private BlockingCollection<TwitchMessage> queue;

        private IList<ITwitchCommandObserver> _observers = new List<ITwitchCommandObserver>();
        private Regex CommandRegex = new Regex("!command[s]{0,1}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private ITwitchIrcClientAdapter _client;

        private Task runner;
        public TwitchCommandParser(
            ILogger<TwitchCommandParser> logger,
            ITwitchMessageSubject subject,
            ITwitchIrcClientAdapter client){
            _logger = logger;
            _subject = subject;
            _client = client;
            subject.Attach(this);
            queue = new BlockingCollection<TwitchMessage>(new ConcurrentQueue<TwitchMessage>());
            runner = Run();
        }

        public void Attach(ITwitchCommandObserver TwitchCommandObserver)
        {
            _observers.Add(TwitchCommandObserver);
        }

        public void Detach(ITwitchCommandObserver TwitchCommandObserver)
        {
            _observers.Remove(TwitchCommandObserver);
        }

        public async Task Update(TwitchMessage Message)
        {
            await Task.Run(() => { queue.Add(Message); });
        }

        private Task Run()
        {
            return Task.Run(()=>{
                TwitchChatCommand command;
                Task task;
                StringBuilder builder;
                List<string> commandList;
                string commandName;
                foreach (var message in queue.GetConsumingEnumerable())
                {
                    command = new TwitchChatCommand(message);
                    var paramCount = command.HasParameters ? command.Parameters.Count : 0;
                    _logger.LogInformation($"Command '{command.Command}' parsed with {paramCount} parameters.");
                    task = NotifyCommandObservers(command);
                    if (CommandRegex.IsMatch(command.Command))
                    {
                        builder = new StringBuilder();
                        builder.Append($"@{command.Message.TwitchUser.DisplayName} my available commands are ");
                        commandList = new List<string>();
                        foreach (var observer in _observers)
                        {
                            commandName = observer.GetPrimaryCommand();
                            if (!string.IsNullOrWhiteSpace(commandName))
                            {
                                commandList.Add(commandName);
                            }
                        }
                        builder.Append(string.Join(" ", commandList.OrderBy(q => q).ToArray()));
                        task = _client.SendPublicChatMessageAsync(builder.ToString(), command.Message.IrcChannel);
                    }
                }
            });
        }

        private async Task NotifyCommandObservers(TwitchChatCommand command)
        {
            await Task.Run(()=>
            {
                foreach (var observer in _observers)
                {
                    if(observer.IsCommandSupported(command.Command))
                    {
                        observer.Update(command);
                    }
                }
            });
        }

        public string GetName()
        {
            return nameof(TwitchCommandParser);
        }
    }
}
