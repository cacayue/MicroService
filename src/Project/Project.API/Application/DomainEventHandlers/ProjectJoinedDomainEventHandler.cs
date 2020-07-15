using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.API.Application.IntegrationEvents;
using Project.Domain.Events;

namespace Project.API.Application.DomainEventHandlers
{
    public class ProjectJoinedDomainEventHandler:INotificationHandler<ProjectJoinedEvent>
    {
        private readonly ICapPublisher _capPublisher;

        public ProjectJoinedDomainEventHandler(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public Task Handle(ProjectJoinedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectJoinedIntegrationEvent()
            {
                Company = notification.Company,
                Introduction = notification.Introduction,
                Contributor = notification.Contributor
            };
            _capPublisher.Publish("finbook.projectapi.projectjoined", @event);
            return Task.CompletedTask;
        }
    }
}