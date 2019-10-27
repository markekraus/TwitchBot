using System.Threading;
using System.Threading.Tasks;

namespace TwitchBot.Interfaces
{
    public interface IIrcClient
    {
        Task<string> ReadMessageAsync();
        Task<string> ReadMessageAsync(CancellationToken Token);
        Task SendIrcMessageAsync(string Message);
        Task SendPublicChatMessageAsync(string User, string Message, string Channel);
    }
}
