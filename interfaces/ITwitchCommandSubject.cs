using TwitchBot.Models;

namespace TwitchBot.Interfaces
{
    public interface ITwitchCommandSubject
    {
        void Attach(ITwitchCommandObserver TwitchCommandObserver);
        void Detach(ITwitchCommandObserver TwitchCommandObserver);
    }
}