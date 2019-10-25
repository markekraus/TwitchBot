using System;
using System.Collections.Generic;

namespace TwitchBot.Models
{
    public class TwitchBadge
    {
        public string Name { get; set; }
        public int Version { get; set; }

        public TwitchBadge(string BadgeString)
        {
            var badgeParts = BadgeString.Split("/");
            Name = badgeParts[0];
            int value;
            int.TryParse(badgeParts[1], out value);
            Version = value;
        }

        public static IList<TwitchBadge> Parse(string BadgeString)
        {
            var output = new List<TwitchBadge>();
            foreach (var badgePart in BadgeString.Split(","))
            {
                output.Add(new TwitchBadge(badgePart));
            }
            return output;
        }
    }
}