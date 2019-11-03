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
    public class ExchangeRateCommand : ITwitchCommandObserver
    {
        private ILogger<ExchangeRateCommand> _logger;
        private ITwitchCommandSubject _subject;
        private ITwitchIrcClientAdapter _ircClient;
        private HttpClient _httpClient;
        private Task runner;

        public const string PrimaryCommand = "!exchangerate";
        private Regex CommandRex = new Regex("!exchangerate|!exchange", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string ApiUri = "https://api.exchangeratesapi.io/latest?base={0}&symbols={0},{1}";

        private JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public ExchangeRateCommand(
            ILogger<ExchangeRateCommand> logger, 
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
                double amount;
                string usage;
                foreach (var command in queue.GetConsumingEnumerable())
                {
                    if (!command.HasParameters || (command.HasParameters && command.Parameters.Count != 2))
                    {
                        usage = $"@{command.Message.TwitchUser.DisplayName} usage: '{PrimaryCommand} <source currency code> <destination currency code>' example: '{PrimaryCommand} USD INR'";
                        task = _ircClient.SendPublicChatMessageAsync(Message: usage, Channel: command.Message.IrcChannel);
                        continue;
                    }
                    result = string.Empty;
                    message = string.Empty;
                    errorMessage = $"Sorry @{command.Message.TwitchUser.DisplayName}, I was unable to get an exchange rate.";
                    _logger.LogInformation($"Processing {command.Command}...");
                    try
                    {
                        result = _httpClient.GetStringAsync(string.Format(ApiUri, command.Parameters[0].ToUpper(), command.Parameters[1].ToUpper())).GetAwaiter().GetResult();
                        _logger.LogInformation($"Received HTTP message: '{result}'");
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "HttpClient Failed");
                    }
                    try
                    {
                        var exchange = JsonSerializer.Deserialize<ExchangeResponse>(result, options);
                        if(exchange.Rates.TryGetValue(command.Parameters[1], out amount)){
                            message = $"1 {command.Parameters[0]} is {amount} {command.Parameters[1]}";
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

        public string GetPrimaryCommand(TwitchChatCommand Command)
        {
            return PrimaryCommand;
        }

        public bool IsCommandSupported(string Command)
        {
            return CommandRex.IsMatch(Command);
        }
    }
}
