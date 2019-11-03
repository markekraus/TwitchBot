using System.Collections.Generic;
using TwitchBot.Models;

namespace TwitchBot.Options
{
    public class AppSettings
    {
        public string BotUsername { get; set; }
        public string BotPassword { get; set; }
        public string BotOwner { get; set; }
        public IList<string> Channels { get; set; }

        public IList<Sound> Sounds { get; set; }
    }
}
