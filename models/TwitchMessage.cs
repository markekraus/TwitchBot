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

        public Guid Id { get; set; }

        public int Bits { get; set; }

        public bool HasBits { get; set; } = false;

        public DateTimeOffset TimeStamp { get; set; }
        public const string AllowedAction = "PRIVMSG";
        public TwitchMessage(string RawMessage) : base(RawMessage)
        { 
            init();
        }

        public TwitchMessage(IrcMessage IrcMessage): base(IrcMessage.RawMessage)
        {
            init();
        }

        private void init()
        {
            if (base.Action != AllowedAction)
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
                IsBroadcaster = true
            };

            ChannelUri = new Uri($"https://twitch.tv/{ChannelOwner.UserName}");

            string roomIdString;
            int roomId;
            if(Tags.TryGetValue("room-id",out roomIdString) && int.TryParse(roomIdString, out roomId))
            {
                RoomId = roomId;
            }

            string idString;
            Guid id;
            if (Tags.TryGetValue("id", out idString) && Guid.TryParse(idString, out id))
            {
                Id = id;
            }

            string bitsString;
            int bits;
            if(Tags.TryGetValue("bits", out bitsString) && int.TryParse(bitsString, out bits))
            {
                Bits = bits;
                HasBits = true;
            }

            string timestampString;
            long timestampLong;
            if (Tags.TryGetValue("tmi-sent-ts", out timestampString) && long.TryParse(timestampString, out timestampLong))
            {
                try
                {
                    TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampLong);
                }
                catch {}
            }
        }
    }
}