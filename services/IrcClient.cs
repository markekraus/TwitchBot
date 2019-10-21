using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

using TwitchBot.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Threading;
using System.Collections.Concurrent;

namespace TwitchBot.Services
{
    // Reference: https://www.youtube.com/watch?v=Ss-OzV9aUZg
    public class IrcClient
    {
        private readonly IrcSettings _config;
        private readonly ILogger _logger;

        private TcpClient tcpClient;
        private SslStream sslStream;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        private BlockingCollection<string> inputQueue;
        private BlockingCollection<string> outputQueue;

        private Task inputTask;
        private Task outputTask;

        private const string hostName = "irc.chat.twitch.tv";
        private const int port = 6697;

        public IrcClient(IOptions<IrcSettings> config, ILogger<IrcClient> logger)
        {
            _config = config.Value;
            _logger = logger;

            inputQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
            outputQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());

            _logger.LogInformation("Connecting to irc", hostName, port);
            tcpClient = new TcpClient(hostName, port);
            sslStream = new SslStream(tcpClient.GetStream());
            _logger.LogInformation("SSL Client Authentication");
            sslStream.AuthenticateAsClient(hostName);
            _logger.LogInformation("Create input and output stream.");
            inputStream = new StreamReader(sslStream);
            outputStream = new StreamWriter(sslStream);

            inputTask = Task.Run(() => {
                foreach (var message in inputQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        _logger.LogInformation($"irc send: {message}");
                        outputStream.WriteLine(message);
                        outputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "IrcClient", "SendIrcMessage(string)", false);
                    }
                }
            });

            outputTask = Task.Run(() => {
                while (true)
                {
                    try
                    {
                        var line = inputStream.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _logger.LogInformation($"irc receive: {line}");
                            outputQueue.TryAdd(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading message");
                    }
                }
            });

            // Reference: https://dev.twitch.tv/docs/irc/tags/
            SendIrcMessageAsync("CAP REQ :twitch.tv/tags").GetAwaiter().GetResult();
            // Reference: https://dev.twitch.tv/docs/irc/commands/
            SendIrcMessageAsync("CAP REQ :twitch.tv/commands").GetAwaiter().GetResult();
            // Reference: https://dev.twitch.tv/docs/irc/membership/
            SendIrcMessageAsync("CAP REQ :twitch.tv/membership").GetAwaiter().GetResult();
            SendIrcMessageAsync("PASS " + _config.Password).GetAwaiter().GetResult();
            SendIrcMessageAsync("NICK " + _config.UserName).GetAwaiter().GetResult();
            SendIrcMessageAsync("USER " + _config.UserName + " 8 * :" + _config.UserName).GetAwaiter().GetResult();
            SendIrcMessageAsync("JOIN #" + _config.Channel).GetAwaiter().GetResult();
        }

        public async Task SendIrcMessageAsync(string message)
        {
            await Task.Run ( () =>
                {
                    try
                    {
                        _logger.LogInformation($"Queing message: {message}");
                        inputQueue.TryAdd(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "IrcClient", "SendIrcMessage(string)", false);
                    }
                }
            );
        }

        public async Task SendPublicChatMessageAsync(string message)
        {
            var msg = ":" + _config.UserName + "!" + _config.UserName + "@" + _config.UserName +
                    ".tmi.twitch.tv PRIVMSG #" + _config.Channel + " :" + message;
            try
            {
                await SendIrcMessageAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sent chat message.", message, msg);
            }
        }

        // public async void ClearMessage(TwitchChatter chatter)
        // {
        //     try
        //     {
        //         SendIrcMessage(":" + _config.UserName + "!" + _config.UserName + "@" + _config.UserName +
        //             ".tmi.twitch.tv PRIVMSG #" + _config.Channel + " :/delete " + chatter.MessageId);
        //     }
        //     catch (Exception ex)
        //     {
        //         await _errHndlrInstance.LogError(ex, "IrcClient", "ClearMessage(TwitchChatter)", false);
        //     }
        // }

        public async Task SendChatTimeoutAsync(string offender, int timeout = 1)
        {
            var msg = ":" + _config.UserName + "!" + _config.UserName + "@" + _config.UserName +
                    ".tmi.twitch.tv PRIVMSG #" + _config.Channel + " :/timeout " + offender + " " + timeout;
            try
            {
                await SendIrcMessageAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to send time out", offender, timeout, msg);
            }
        }

        public async Task<string> ReadMessageAsync()
        {
            return await Task.Run<string>(() => {
                string line = string.Empty;
                try
                {
                    line = outputQueue.Take();
                    _logger.LogInformation($"message dequeued: {line}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading message");
                }
                return line;
            });
        }
    }
}
