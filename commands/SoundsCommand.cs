using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchBot.Interfaces;
using TwitchBot.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using TwitchBot.Options;
using System.Text;
using LibVLCSharp.Shared;
using System.Threading;

namespace TwitchBot.Commands
{
    public class SoundsCommand : ITwitchCommandObserver
    {
        private ILogger<SoundsCommand> _logger;
        private ITwitchCommandSubject _subject;
        private ITwitchIrcClientAdapter _ircClient;
        private Task _runner;
        private SoundsCommandSettings _config;
        private LibVLC _libVLC;
        private MediaPlayer _player;

        public const string PrimaryCommand = "!sounds";
        private Regex _commandRex = new Regex("!sound", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private BlockingCollection<TwitchChatCommand> queue = new BlockingCollection<TwitchChatCommand>(new ConcurrentQueue<TwitchChatCommand>());
        public SoundsCommand(
            ILogger<SoundsCommand> logger, 
            ITwitchCommandSubject subject, 
            ITwitchIrcClientAdapter ircClient,
            IOptions<SoundsCommandSettings> config)
        {
            _logger = logger;
            _subject = subject;
            _ircClient = ircClient;
            _config = config.Value;

            Core.Initialize();
            _libVLC = new LibVLC();
            _player = new MediaPlayer(_libVLC);

            subject.Attach(this);
            _runner = Run();
        }

        public async Task Update(TwitchChatCommand Command)
        {
            await Task.Run(() => { queue.Add(Command); });
        }

        private Task Run()
        {
            return Task.Run(()=>
            {
                Regex regex;
                string channel;
                foreach (var command in queue.GetConsumingEnumerable())
                {
                    channel = command.Message.ChannelOwner.UserName.ToLower();
                    if(channel != _config.SoundsChannel.ToLower())
                    {
                        _logger.LogInformation($"Command not sent from correct chan. SoundsChan: '{_config.SoundsChannel}', SourceChan: {channel}");
                        continue;
                    }
                    if (_commandRex.IsMatch(command.Command))
                    {
                        _ircClient.SendPublicChatMessageAsync(Message: GetDefaultCommandMessage(), Channel: command.Message.IrcChannel);
                        continue;
                    }

                    foreach (var sound in _config.Sounds)
                    {
                        if (!sound.Enabled)
                        {
                            continue;
                        }

                        if (string.Equals(command.Command, sound.PrimaryCommand, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _logger.LogInformation($"playing {sound.PrimaryCommand}");
                            Play(sound.FilePath);
                            break;
                        }

                        try
                        {
                            regex = new Regex(sound.CommandRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            if(regex.IsMatch(command.Command))
                            {
                                _logger.LogInformation($"playing {sound.CommandRegex}");
                                Play(sound.FilePath);
                                break;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogError(ex, $"Unable to parse regex '{sound.CommandRegex}' for sound command '{sound.PrimaryCommand}'.");
                        }
                    }
                }
            });
        }

        private void Play(string filePath)
        {
            _logger.LogInformation($"playing file {filePath}");
            var uri = new Uri(filePath);
            using (var media = new Media(_libVLC, uri.AbsoluteUri, FromType.FromLocation))
            {
                _player.Play(media);
                Thread.Sleep(100);
                while(_player.State == VLCState.Playing){  }
            } 
        }

        private string GetDefaultCommandMessage()
        {
            var builder = new StringBuilder("My available sound commands are");
            foreach (var sound in _config.Sounds)
            {
                if (sound.Visible && !String.IsNullOrWhiteSpace(sound.PrimaryCommand))
                {
                    builder.Append(" ").Append(sound.PrimaryCommand.Trim());
                }
            }
            return builder.ToString();
        }

        public string GetPrimaryCommand(TwitchChatCommand Command)
        {
            return PrimaryCommand;
        }

        public bool IsCommandSupported(string Command)
        {
            if (_commandRex.IsMatch(Command))
            {
                return true;
            }
            Regex regex;
            foreach (var sound in _config.Sounds)
            {
                if (!sound.Enabled)
                {
                    continue;
                }
                if (string.Equals(Command, sound.PrimaryCommand, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                try
                {
                    regex = new Regex(sound.CommandRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if(regex.IsMatch(Command))
                    {
                        return true;
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"Unable to parse regex '{sound.CommandRegex}' for sound command '{sound.PrimaryCommand}'.");
                }
            }
            return false;
        }
    }
}
