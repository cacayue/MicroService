using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;
using Project.Domain.Exceptions;

namespace Project.API.Application.Commands
{
    public class ViewProjectCommandHandler : AsyncRequestHandler<ViewProjectCommand>
    {
        private readonly IProjectRepository _projectRepository;

        public ViewProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task Handle(ViewProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetAsync(request.ProjectId, cancellationToken);
            if (project == null)
            {
                throw new ProjectDomainException($"project not found: {request.ProjectId}");
            }
            if (project.UserId == request.UserId)
            {
                throw new ProjectDomainException($"you can't view your own project");
            }
            project.AddViewer(request.UserId,request.UserName,request.Avatar);
            await _projectRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }
}