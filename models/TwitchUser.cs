using System;
using System.Collections.Generic;
using System.Linq;

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
        public string Color { get; set; }

        public int UserId { get; set; }

        public bool IsMod { get; set; } = false;

        public bool IsVip { get; set; } = false;
        public bool IsSubGifter { get; set; } = false;

        public bool IsBroadcaster { get; set; } = false;
        public bool IsFounder { get; set; } = false;
        public bool IsSubscriber { get; set; } = false;

        public string IrcUserName
        { 
            get
            {
                if(String.IsNullOrWhiteSpace(_ircUserName))
                {
                    _ircUserName =  $":{UserName}!{UserName}@{UserName}.tmi.twitch.tv"; 
                }
                return _ircUserName;
            }
        }
        private string _ircUserName;

        public IList<TwitchBadge> BadgeInfo;
        public IList<TwitchBadge> Badges;

        public IrcMessage IrcMessage { get; set; }

        public TwitchUser(IrcMessage Message)
        {
            IrcMessage = Message;
            UserName = Message.Source.Split("!")[0];

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
                    IsVip = Badges.Where(x => { return x.Name == "vip";}).Count() > 0;
                    IsSubGifter = Badges.Where(x => { return x.Name == "sub-gifter";}).Count() > 0;
                    IsMod = Badges.Where(x => { return x.Name == "mod";}).Count() > 0;
                    IsFounder = Badges.Where(x => { return x.Name == "founder";}).Count() > 0;
                    IsSubscriber = Badges.Where(x => { return x.Name == "subscriber";}).Count() > 0;
                    IsBroadcaster = Badges.Where(x => { return x.Name == "broadcaster";}).Count() > 0;
                }

                if(!IsMod && IsBroadcaster) { IsMod = true; }

                string color;
                if (Message.Tags.TryGetValue("color", out color))
                {
                    Color = color;
                }
            }
        }

        public TwitchUser() {}

        public override string ToString()
        {
            return $"{UserName}";
        }
    }
}
