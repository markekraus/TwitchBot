using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchBot.Models
{
    public class TwitchChatCommand
    {
        public string Command { get; set; }
        public IList<string> Parameters { get; set; }
        public TwitchMessage Message { get; set; }
        public bool HasParameters
        {
            get
            {
                return Parameters != null && Parameters.Count > 0;
            }
        }

        public TwitchChatCommand(TwitchMessage message)
        {
            Message = message;
            Parse(message.Message);
        }

        public TwitchChatCommand(string message)
        {
            Parse(message);
        }

        private void Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            message = message.Trim();
            var spaceIndex = message.IndexOf(" ");
            if (spaceIndex < 0)
            {
                Command = message;
                return;
            }

            Command = message.Substring(0, spaceIndex);

            Parameters = new List<string>();

            bool inQuotes = false;
            StringBuilder builder = new StringBuilder();
            string param = string.Empty;
            foreach (var c in message.Substring(spaceIndex))
            {
                if (c == '"' || c == '“' || c == '”')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (!inQuotes && c == ' ')
                {
                    param = builder.ToString();
                    builder = new StringBuilder();
                }
                if (c != ' ' || inQuotes)
                {
                    builder.Append(c);
                }
                if (!string.IsNullOrEmpty(param))
                {
                    Parameters.Add(param);
                    param = string.Empty;
                }
            }
            param = builder.ToString();
            if (!string.IsNullOrEmpty(param))
            {
                Parameters.Add(param);
            }
        }
    }
}