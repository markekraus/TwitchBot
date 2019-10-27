using System.Collections.Generic;
using TwitchBot.Models;

namespace TwitchBot.Options
{
    public class TwitchIrcClientAdapterSettings
    {
        public TwitchUser BotUser { get; set; }
        public IList<string> Channels { get; set; }
    }
}
