using System.Threading.Tasks;

namespace TwitchBot.Interfaces
{
    public interface IIrcClientObserver
    {
        Task Update(string Message);
    }
}