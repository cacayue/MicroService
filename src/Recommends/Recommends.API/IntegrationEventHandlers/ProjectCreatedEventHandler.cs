using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Recommends.API.Data;
using Recommends.API.IntegrationEvents;
using Recommends.API.Models;
using Recommends.API.Services;

namespace Recommends.API.IntegrationEventHandlers
{
    public class ProjectCreatedEventHandler: IProjectCreatedEventHandler, ICapSubscribe
    {
        private readonly RecommendDbContext _context;
        private readonly IUserService _userService;
        private readonly IContactService _contactService;

        public ProjectCreatedEventHandler(RecommendDbContext context,IUserService userService,
            IContactService contactService)
        {
            _context = context;
            _userService = userService;
            _contactService = contactService;
        }
        [CapSubscribe("finbook.projectapi.projectcreated")]
        public async Task CreatedRecommendFromProject(ProjectCreatedIntegrationEvent @event)
        {
            var fromUser =await _userService.GetBaseUserInfoAsync(@event.UserId);
            var contacts = await _contactService.GetContactsByUserId(@event.UserId);
            foreach (var contact in contacts)
            {
                var recommends = new Recommend()
                {
                    UserId = contact.UserId,
                    Company = @event.Company,
                    CreatedTime = @event.CreatedTime,
                    FinStage = @event.FinStage,
                    FromUserId = @event.UserId,
                    Introduction = @event.Introduction,
                    ProjectId = @event.ProjectId,
                    RecommendTime = DateTime.Now,
                    RecommendType = EnumRecommendType.Friend,
                    Tags = @event.Tags,
                    FromUserAvatar = fromUser.Avatar,
                    FromUserName = fromUser.Name,
                };

                await _context.Recommends.AddAsync(recommends);
            }
            await _context.SaveChangesAsync();
        }
    }
}