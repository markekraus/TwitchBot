namespace TwitchBot.Options
{
    public class IrcSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DefaultChannel { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public bool EnableTls { get; set; } = false;
        public int MaxRetryAttempts { get; set; } = 20;
    }
}
