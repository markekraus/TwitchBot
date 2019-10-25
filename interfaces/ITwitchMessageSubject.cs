using TwitchBot.Models;

namespace TwitchBot.Interfaces
{
    public interface ITwitchMessageSubject
    {
        void Attach(ITwitchMessageObserver TwitchMessageObserver);
        void Detach(ITwitchMessageObserver TwitchMessageObserver);
    }
}