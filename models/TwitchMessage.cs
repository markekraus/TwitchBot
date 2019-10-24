using System;
using System.Collections.Generic;
using System.Net;

namespace TwitchBot.Models
{
    public class TwitchMessage : IrcMessage
    {
        public TwitchUser TwitchUser { get; set; }
        public int RoomId { get; set; }
        public string IrcChannel { 
            get
            {
                return Target;
            }
         }
        public Uri ChannelUri { get; set; }

        public TwitchUser ChannelOwner { get; set; }
        private const string _allowedAction = "PRIVMSG";
        public TwitchMessage(string RawMessage) : base(RawMessage)
        { 
            if (base.Action != _allowedAction)
            {
                throw new InvalidCastException($"Cannot convert to {nameof(TwitchMessage)}: {base.RawMessage}");
            }
            if (!base.HasTags)
            {
                throw new InvalidCastException($"Cannot convert to {nameof(TwitchMessage)}: No tags present");
            }

            TwitchUser = new TwitchUser(this);

            ChannelOwner = new TwitchUser()
            {
                UserName = Target.Substring(1),
                IsOwner = true
            };

            ChannelUri = new Uri($"https://twitch.tv/{ChannelOwner.UserName}");

            string roomIdString;
            int roomId;
            if(Tags.TryGetValue("room-id",out roomIdString) && int.TryParse(roomIdString, out roomId))
            {
                RoomId = roomId;
            }
        }
    }
}