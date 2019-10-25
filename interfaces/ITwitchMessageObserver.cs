using System.Threading.Tasks;
using TwitchBot.Models;

namespace TwitchBot.Interfaces
{
    public interface ITwitchMessageObserver
    {
        Task Update(TwitchMessage Message);
    }
}