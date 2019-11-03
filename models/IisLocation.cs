using System.Text.Json.Serialization;

namespace TwitchBot.Models
{
    public class IisLocation
    {
        public string Message { get; set; }
        public int timestamp { get; set; }
        [JsonPropertyName("iss_position")]
        public IisPosition IisPosition  { get; set; }

        public IisLocation() {}
    }
}
