using System.Threading.Tasks;
using Contact.API.IntegrationEvents.Events;

namespace Contact.API.IntegrationEvents.EventHandling
{
    public interface ISubscriberService
    {
        public Task CheckReceivedMessage(UserProfileChangedEvent user);
    }
}