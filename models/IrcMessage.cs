using System;
using System.Collections.Generic;
using System.Net;

namespace TwitchBot.Models
{
    public class IrcMessage
    {
        public string RawMessage { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Action { get; set; }

        public IDictionary<string,string> Tags { get; set; }

        public bool HasTags {
            get
            {
                return Tags != null && Tags.Count > 0;
            }
        }

        private const string _ircMessageSeperator = " :";
        private const string _ircMessageIndicator = ":";
        private const string _ircTagIndicator = "@";
        private const string _ircTagSeperator = ";";
        private const string _ircTagKeySeperator = "=";
        private const string _ircContextSeperator = " ";
        private const string _ircPingIndicator = "PING";
        private const string _parseException = "Unable to parse IRC Message: {0}";


        public IrcMessage() {}
        public IrcMessage (string RawMessage)
        {
            Parse(RawMessage);
        }

        private void Parse(string rawMessage)
        {
            RawMessage = rawMessage;
            string context;
            int seperatorIndex;
            if(rawMessage.StartsWith(_ircTagIndicator))
            {
                Tags = new Dictionary<string,string>();
                seperatorIndex = rawMessage.IndexOf(_ircMessageSeperator);
                var tagEndIndex = seperatorIndex - 1;
                var contextStartIndex = seperatorIndex + 1;
                var tagString = rawMessage.Substring(1,tagEndIndex);

                foreach (var tagPart in tagString.Split(_ircTagSeperator))
                {
                    var tagParts = tagPart.Split(_ircTagKeySeperator);
                    Tags.Add(tagParts[0],tagParts[1]);
                }
                rawMessage = rawMessage.Substring(contextStartIndex);
            }
            if(rawMessage.StartsWith(_ircMessageIndicator))
            {
                rawMessage = rawMessage.Substring(1);
                seperatorIndex = rawMessage.IndexOf(_ircMessageSeperator);
                var contextEndIndex = seperatorIndex - 1;
                var messageStartIndex = seperatorIndex + 2;
                if(rawMessage.IndexOf(_ircMessageSeperator) < 0)
                {
                    context = rawMessage;
                }
                else
                {
                    context = rawMessage.Substring(0,seperatorIndex);
                    Message = rawMessage.Substring(messageStartIndex);
                }
                if (!string.IsNullOrWhiteSpace(context))
                {
                    var contextParts = context.Split(_ircContextSeperator,3);
                    Source = contextParts[0];
                    Action = contextParts[1];
                    Target = contextParts[2];
                }
            }
            else if (rawMessage.StartsWith(_ircPingIndicator))
            {
                Source = rawMessage.Substring(rawMessage.IndexOf(_ircContextSeperator));
                Action = _ircPingIndicator;
                Target = Dns.GetHostName();
            }
            else
            {
                throw new FormatException(string.Format(_parseException,RawMessage));
            }
        }

        public override string ToString()
        {
            var output = $"Source: {Source}; Target: {Target}; Action: {Action}; Message: {Message}";
            return output;
        }
    }
}