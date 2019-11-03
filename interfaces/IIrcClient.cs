using System.Threading;
using System.Threading.Tasks;

namespace TwitchBot.Interfaces
{
    public interface IIrcClient
    {
        void Attach(IIrcClientObserver IrcClientObserver);
        Task SendIrcMessageAsync(string Message);
        Task JoinChannelAsync(string Message);
        Task SendPublicChatMessageAsync(string User, string Message, string Channel);
    }
}
