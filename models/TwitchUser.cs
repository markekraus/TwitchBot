using System;
using System.Collections.Generic;

namespace TwitchBot.Models
{
    public class TwitchUser
    {
        public string UserName { get; set; }
        public string IrcChannel { 
            get 
            {
                if(string.IsNullOrWhiteSpace(_ircChannel))
                {
                    _ircChannel = $"#{UserName}";
                }
                return _ircChannel;
            }
        }
        private string _ircChannel;
        public Uri ChannelUri {
            get
            {
                if(_channelUri == null)
                {
                    _channelUri = new Uri($"https://twitch.tv/{UserName}");
                }
                return _channelUri;
            }
        }
        private Uri _channelUri;
        public string DisplayName { get; set; }
        public string UserType { get; set; }

        public int UserId { get; set; }

        public bool IsMod {
            get
            {
                return UserType == "mod";
            }
        }

        public bool IsOwner { get; set; }

        public IList<TwitchBadge> BadgeInfo;
        public IList<TwitchBadge> Badges;

        public IrcMessage IrcMessage { get; set; }

        public TwitchUser(IrcMessage Message)
        {
            IrcMessage = Message;
            UserName = Message.Source.Split("!")[0];

            if (Message.Target.StartsWith("#") && UserName == Message.Target.Substring(1))
            {
                    IsOwner = true;
            }

            if(Message.HasTags)
            {
                string userType;
                Message.Tags.TryGetValue("user-type", out userType);
                UserType = userType;

                string displayName;
                if(Message.Tags.TryGetValue("display-name", out displayName))
                {
                    DisplayName = displayName;
                }
                else
                {
                    DisplayName = UserName;
                }

                string userIdString;
                if(Message.Tags.TryGetValue("user-id", out userIdString))
                {
                    int userId;
                    int.TryParse(userIdString, out userId);
                    UserId = userId;
                }

                string badgeInfo;
                if (Message.Tags.TryGetValue("badge-info", out badgeInfo) && ! string.IsNullOrWhiteSpace(badgeInfo))
                {
                    BadgeInfo = TwitchBadge.Parse(badgeInfo);
                }

                string badges;
                if (Message.Tags.TryGetValue("badges", out badges) && ! string.IsNullOrWhiteSpace(badges))
                {
                    Badges = TwitchBadge.Parse(badges);
                }
            }
        }

        public TwitchUser() {}
    }
}
