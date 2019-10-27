using System.Threading.Tasks;

namespace TwitchBot.Interfaces
{
    public interface ITwitchIrcClientAdapter
    {
        Task SendPublicChatMessageAsync(string Message, string Channel);
    }
}
