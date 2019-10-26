using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;
using TwitchBot.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitchBot.Commands
{
    public class CatfactsCommand : ITwitchCommandObserver
    {
        private ILogger<TestCommand> _logger;
        private ITwitchCommandSubject _subject;
        private IrcClient _ircClient;
        private HttpClient _httpClient;
        private Task runner;

        public const string Command = "!catfact";
        private static Uri CatfactUri = new Uri("https://catfact.ninja/fact");

        private JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public CatfactsCommand(
            ILogger<TestCommand> logger, 
            ITwitchCommandSubject subject, 
            IrcClient ircClient,
            HttpClient httpClient)
        {
            _logger = logger;
            _subject = subject;
            _ircClient = ircClient;
            _httpClient = httpClient;

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
                string errorMessage;
                string result;
                foreach (var command in queue.GetConsumingEnumerable())
                {
                    result = string.Empty;
                    message = string.Empty;
                    errorMessage = $"Sorry @{command.Message.TwitchUser.DisplayName}, I was unable to get a cat fact.";
                    _logger.LogInformation($"Processing {command.Command}...");
                    try
                    {
                        result = _httpClient.GetStringAsync(CatfactUri).GetAwaiter().GetResult();
                        _logger.LogInformation($"Received HTTP message: '{result}'");
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "HttpClient Failed");
                    }
                    try
                    {
                        message = JsonSerializer.Deserialize<CatFact>(result, options).Fact;
                        _logger.LogInformation($"Parsed Fact: '{message}'");
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "JSON Deserialization Failed");
                    }
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = errorMessage;
                        _logger.LogError("Message is null or white space.");
                    }
                    else
                    {
                        message = $"@{command.Message.TwitchUser.DisplayName} : {message}";
                    }
                    task = _ircClient.SendPublicChatMessageAsync(message);
                }
            });
        }

        public string GetCommand()
        {
            return Command;
        }
    }
}