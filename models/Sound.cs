namespace TwitchBot.Models
{
    public class Sound
    {
        public string PrimaryCommand { get; set; }
        public string CommandRegex { get; set; }
        public string FilePath { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
    }
}
