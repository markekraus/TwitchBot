using TwitchBot.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchBot.Models;
using System.Threading.Tasks;
using System;

namespace TwitchBot.Services
{

    public class TwitchIrcClientAdapter : ITwitchIrcClientAdapter
    {
        private IIrcClient _client;
        private ILogger<TwitchIrcClientAdapter> _logger;
        private TwitchUser _botUser;

        public TwitchIrcClientAdapter(
            IIrcClient Client,
            ILogger<TwitchIrcClientAdapter> Logger,
            IOptions<TwitchUser> BotUser)
        {
            _client = Client;
            _logger = Logger;
            _botUser = BotUser.Value;

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
        }

        public async Task SendPublicChatMessageAsync(string Message, string Channel)
        {
            await _client.SendPublicChatMessageAsync(User: _botUser.IrcUserName, Message: Message, Channel: Channel);
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
