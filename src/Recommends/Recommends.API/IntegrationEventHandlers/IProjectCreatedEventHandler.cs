using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Recommends.API.Data;
using Recommends.API.IntegrationEvents;
using Recommends.API.Models;
using Recommends.API.Services;

namespace Recommends.API.IntegrationEventHandlers
{
    public interface IProjectCreatedEventHandler
    {
        Task CreatedRecommendFromProject(ProjectCreatedIntegrationEvent @event);
    }
}