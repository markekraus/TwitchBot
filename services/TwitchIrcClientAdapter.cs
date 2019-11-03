using TwitchBot.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchBot.Models;
using System.Threading.Tasks;
using System;
using TwitchBot.Options;

namespace TwitchBot.Services
{

    public class TwitchIrcClientAdapter : ITwitchIrcClientAdapter, IIrcClientObserver
    {
        private IIrcClient _client;
        private ILogger<TwitchIrcClientAdapter> _logger;
        private TwitchIrcClientAdapterSettings _config;

        public TwitchIrcClientAdapter(
            IIrcClient Client,
            ILogger<TwitchIrcClientAdapter> Logger,
            IOptions<TwitchIrcClientAdapterSettings> Config)
        {
            _client = Client;
            _logger = Logger;
            _config = Config.Value;

            _client.Attach(this);

            var init = Init();
        }

        private async Task Init()
        {
            // Reference: https://dev.twitch.tv/docs/irc/tags/
            // Reference: https://dev.twitch.tv/docs/irc/commands/
            // Reference: https://dev.twitch.tv/docs/irc/membership/
            await _client.SendIrcMessageAsync("CAP REQ :twitch.tv/tags");
            await _client.SendIrcMessageAsync("CAP REQ :twitch.tv/commands");
            await _client.SendIrcMessageAsync("CAP REQ :twitch.tv/membership");
            foreach (var channel in _config.Channels)
            {
                await _client.JoinChannelAsync(channel);
            }
        }

        public async Task SendPublicChatMessageAsync(string Message, string Channel)
        {
            await _client.SendPublicChatMessageAsync(User: _config.BotUser.IrcUserName, Message: Message, Channel: Channel);
        }

        public Task Update(string Message)
        {
            return Task.Run(() => {});
        }

        public async Task Reconnect()
        {
            await Init();
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

        // public async Task SendChatTimeoutAsync(string Offender, int Timeout = 1)
        // {
        //     var msg = ":" + _config.UserName + "!" + _config.UserName + "@" + _config.UserName +
        //             ".tmi.twitch.tv PRIVMSG #" + _config.DefaultChannel + " :/timeout " + Offender + " " + Timeout;
        //     try
        //     {
        //         await SendIrcMessageAsync(msg);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "failed to send time out", Offender, Timeout, msg);
        //     }
        // }
    }
}
