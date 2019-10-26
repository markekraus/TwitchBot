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
using System.Text.RegularExpressions;

namespace TwitchBot.Commands
{
    public class IssLocationCommand : ITwitchCommandObserver
    {
        private ILogger<IssLocationCommand> _logger;
        private ITwitchCommandSubject _subject;
        private IrcClient _ircClient;
        private HttpClient _httpClient;
        private Task runner;

        public const string PrimaryCommand = "!catfact";
        private Regex CommandRex = new Regex("[!]{0,1}iss|[!]{0,1}issloc|[!]{0,1}isslocation|whereisiss|isswhere", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Uri CatfactUri = new Uri("http://api.open-notify.org/iss-now.json");

        private JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public IssLocationCommand(
            ILogger<IssLocationCommand> logger, 
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
                        var loc = JsonSerializer.Deserialize<IisLocation>(result, options);
                        message = $"The international Space Station is currently at Longitude: {loc.IisPosition.Longitude}, Latitude: {loc.IisPosition.Latitude}.";
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
                        message = $"@{command.Message.TwitchUser.DisplayName} {message}";
                    }
                    task = _ircClient.SendPublicChatMessageAsync(message);
                }
            });
        }

        public string GetPrimaryCommand()
        {
            return PrimaryCommand;
        }

        public bool IsCommandSupported(string Command)
        {
            return CommandRex.IsMatch(Command);
        }
    }
}