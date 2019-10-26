using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private Task runner;
        public TwitchCommandParser(
            ILogger<TwitchCommandParser> logger,
            ITwitchMessageSubject subject){
            _logger = logger;
            _subject = subject;
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
                foreach (var message in queue.GetConsumingEnumerable())
                {
                    command = new TwitchChatCommand(message);
                    var paramCount = command.HasParameters ? command.Parameters.Count : 0;
                    _logger.LogInformation($"Command '{command.Command}' parsed with {paramCount} parameters.");
                    task = NotifyCommandObservers(command);
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