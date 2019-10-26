using System.Collections.Generic;

namespace TwitchBot.Models
{
    internal class ExchangeResponse
    {
        public IDictionary<string, double> Rates { get; set; }
    }
}