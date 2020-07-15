using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Project.Domain.AggregatesModel;

namespace Project.API.Application.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand,Project.Domain.AggregatesModel.Project>
    {
        private readonly IProjectRepository _projectRepository;

        public CreateOrderCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<Domain.AggregatesModel.Project> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _projectRepository.Add(request.Project);
            await _projectRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return request.Project;
        }
    }
}