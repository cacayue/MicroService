using System.Threading;
using System.Threading.Tasks;
using Contact.API.Data.Repository;
using Contact.API.Dtos;
using Contact.API.IntegrationEvents.Events;
using DotNetCore.CAP;
using Microsoft.Extensions.Primitives;

namespace Contact.API.IntegrationEvents.EventHandling
{
    public class UserProfileChangedEventHandler: ISubscriberService,ICapSubscribe
    {
        private readonly IContactRepository _contactRepository;

        public UserProfileChangedEventHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [CapSubscribe("finbook.userapi.userprofilechanged")]
        public async Task CheckReceivedMessage(UserProfileChangedEvent user)
        {
            var token = new CancellationToken();
            await _contactRepository.UpdateContactInfoAsync(new BaseUserInfo
            {
                Id = user.UserId,
                Avatar = user.Avatar,
                Company = user.Company,
                Name = user.Name,
                Title = user.Title
            }, token);
        }
    }
}