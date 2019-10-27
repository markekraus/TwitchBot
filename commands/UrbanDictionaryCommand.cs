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
    public class UrbanDictionaryCommand : ITwitchCommandObserver
    {
        private ILogger<UrbanDictionaryCommand> _logger;
        private ITwitchCommandSubject _subject;
        private ITwitchIrcClientAdapter _ircClient;
        private HttpClient _httpClient;
        private Task runner;

        public const string PrimaryCommand = "!urbandictionary";
        private Regex CommandRex = new Regex("!urban|!urbandict|!urbandictionary!define", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string ApiUri = "https://api.urbandictionary.com/v0/define?term={0}";

        private JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public UrbanDictionaryCommand(
            ILogger<UrbanDictionaryCommand> logger, 
            ITwitchCommandSubject subject, 
            ITwitchIrcClientAdapter ircClient,
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
                    var usage = $"@{command.Message.TwitchUser.DisplayName} usage: '{PrimaryCommand} <word>' example: '{PrimaryCommand} face' or '{PrimaryCommand} \"double negative\"'";
                    if (!command.HasParameters || (command.HasParameters && command.Parameters.Count != 1))
                    {
                        task = _ircClient.SendPublicChatMessageAsync(
                            Message: usage, 
                            Channel: command.Message.IrcChannel);
                        continue;
                    }
                    result = string.Empty;
                    message = string.Empty;
                    errorMessage = $"Sorry @{command.Message.TwitchUser.DisplayName}, I was unable to get an Urban Dictionary Definition. {usage}";
                    _logger.LogInformation($"Processing {command.Command}...");
                    try
                    {
                        result = _httpClient.GetStringAsync(string.Format(ApiUri, command.Parameters[0])).GetAwaiter().GetResult();
                        _logger.LogInformation($"Received HTTP message: '{result}'");
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "HttpClient Failed");
                    }
                    try
                    {
                        var definition = JsonSerializer.Deserialize<UrbanDictionaryResponse>(result, options);
                        if(!string.IsNullOrWhiteSpace(definition.List[0].definition)){
                            message = Regex.Replace(definition.List[0].definition, @"\r\n?|\n|\r", " ");
                            message = $"{command.Parameters[0]} - {message}";
                            _logger.LogInformation($"Parsed Fact: '{message}'");
                        }
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
                    task = _ircClient.SendPublicChatMessageAsync(Message: message, Channel: command.Message.IrcChannel);
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
