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
using TwitchBot.Interfaces;

namespace TwitchBot.Services
{

    // Reference: https://www.youtube.com/watch?v=Ss-OzV9aUZg
    public class IrcClient : IIrcClient
    {
        private readonly IrcSettings _config;
        private readonly ILogger _logger;

        private TcpClient _tcpClient;
        private SslStream _sslStream;
        private StreamReader _inputStream;
        private StreamWriter _outputStream;

        private BlockingCollection<string> inputQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
        private BlockingCollection<string> outputQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());

        private Task _inputTask;
        private Task _outputTask;

        private int _retryCount = 0;

        public IrcClient(
            IOptions<IrcSettings> config,
            ILogger<IrcClient> logger)
        {
            _config = config.Value;
            _logger = logger;

            Connect();

            _inputTask = StartInputHandler();

            _outputTask = StartOutputHandler();
        }

        private Task StartInputHandler()
        {
            return Task.Run(() =>
            {
                foreach (var message in inputQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        _logger.LogInformation($"irc send: {message}");
                        _outputStream.WriteLine(message);
                        _outputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "IrcClient", "SendIrcMessage(string)", false);
                    }
                }
            });
        }

        private Task StartOutputHandler()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var line = _inputStream.ReadLine();
                        _retryCount = 0;
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _logger.LogInformation($"irc receive: {line}");
                            outputQueue.TryAdd(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading message");
                        Reconnect();
                    }
                }
            });
        }

        private void Connect()
        {
            _logger.LogInformation("Connecting to irc", _config.HostName, _config.Port);
            _tcpClient = new TcpClient(_config.HostName, _config.Port);
            _sslStream = new SslStream(_tcpClient.GetStream());
            _logger.LogInformation("SSL Client Authentication");
            _sslStream.AuthenticateAsClient(_config.HostName);
            _logger.LogInformation("Create input and output stream.");
            _inputStream = new StreamReader(_sslStream);
            _outputStream = new StreamWriter(_sslStream);

            _outputStream.WriteLine("PASS " + _config.Password);
            _outputStream.Flush();
            _outputStream.WriteLine("NICK " + _config.UserName);
            _outputStream.Flush();
            _outputStream.WriteLine("USER " + _config.UserName + " 8 * :" + _config.UserName);
            _outputStream.Flush();
            _outputStream.WriteLine("JOIN #" + _config.DefaultChannel);
            _outputStream.Flush();
        }

        public async Task JoinChannelAsync(string Channel)
        {
            if(Channel.StartsWith("#"))
            {
                Channel = Channel.Substring(1);
            }
            _logger.LogInformation($"Joining channel '{Channel}'");
            await SendIrcMessageAsync($"JOIN #{Channel}");
        }

        private void Reconnect()
        {
            if (_retryCount < _config.MaxRetryAttempts)
            {
                _retryCount++;
                _logger.LogInformation($"Attempting to reconnect. Attempt {_retryCount} of {_config.MaxRetryAttempts}.");
                Connect();
            }
            else
            {
                Environment.Exit(1);
            }
        }

        public async Task SendIrcMessageAsync(string Message)
        {
            await Task.Run(() =>
              {
                  try
                  {
                      _logger.LogInformation($"Queing message: {Message}");
                      inputQueue.TryAdd(Message);
                  }
                  catch (Exception ex)
                  {
                      _logger.LogError(ex, "IrcClient", "SendIrcMessage(string)", false);
                  }
              }
            );
        }

        public async Task SendPublicChatMessageAsync(string User, string Message, string Channel)
        {
            if (Channel.StartsWith("#"))
            {
                Channel = Channel.Substring(1);
            }

            var msg = $"{User} PRIVMSG #{Channel} :{Message}";
            try
            {
                await SendIrcMessageAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sent chat message.", Message, msg);
            }
        }

        public async Task<string> ReadMessageAsync()
        {
            var token = new CancellationTokenSource().Token;
            return await ReadMessageAsync(token);
        }
        public async Task<string> ReadMessageAsync(CancellationToken Token)
        {
            return await Task.Run<string>(() =>
            {
                string line = string.Empty;
                try
                {
                    line = outputQueue.Take(Token);
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
