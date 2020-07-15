using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.API.Application.IntegrationEvents;
using Project.Domain.Events;

namespace Project.API.Application.DomainEventHandlers
{
    public class ProjectCreatedDomainEventHandler:INotificationHandler<ProjectCreatedEvent>
    {
        private readonly ICapPublisher _capPublisher;

        public ProjectCreatedDomainEventHandler(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectCreatedIntegrationEvent
            {
                UserId = notification.Project.UserId,
                ProjectId = notification.Project.Id,
                Company = notification.Project.Company,
                FinStage = notification.Project.FinStage,
                Introduction = notification.Project.Introduction,
                Tags = notification.Project.Tags,
                CreatedTime = DateTime.Now
            };
            _capPublisher.Publish("finbook.projectapi.projectcreated", @event);
            return Task.CompletedTask;
        }
    }
}