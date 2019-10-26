using System.Threading.Tasks;
using TwitchBot.Models;

namespace TwitchBot.Interfaces
{
    public interface IIrcMessageObserver
    {
        Task Update(IrcMessage Message);
        string GetName();
    }
}