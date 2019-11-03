using System.Collections.Generic;
using TwitchBot.Models;

namespace TwitchBot.Options
{
    public class SoundsCommandSettings
    {
        public string SoundsChannel { get; set; }
        public IList<Sound> Sounds { get; set; }
    }
}
