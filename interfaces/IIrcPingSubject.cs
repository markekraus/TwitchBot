using TwitchBot.Models;

namespace TwitchBot.Interfaces
{
    public interface IIrcMessageSubject
    {
        void Attach(IIrcMessageObserver IrcMessageObserver);
        void Detach(IIrcMessageObserver IrcMessageObserver);
    }
}