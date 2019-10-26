using System.Threading.Tasks;
using TwitchBot.Models;

namespace TwitchBot.Interfaces
{
    public interface ITwitchCommandObserver
    {
        Task Update(TwitchChatCommand Command);
        string GetPrimaryCommand();

        bool IsCommandSupported(string Command);
    }
}