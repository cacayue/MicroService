using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;
using Project.Domain.Exceptions;

namespace Project.API.Application.Commands
{
    public class JoinProjectCommandHandler: AsyncRequestHandler<JoinProjectCommand>
    {
        private readonly IProjectRepository _projectRepository;

        public JoinProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task Handle(JoinProjectCommand request, CancellationToken cancellationToken)
        {
           var project =  await _projectRepository.GetAsync(request.Contributor.ProjectId, cancellationToken);
           if (project == null)
           {
               throw new ProjectDomainException($"project not found: {request.Contributor.ProjectId}");
           }

           if (project.UserId == request.Contributor.UserId)
           {
               throw new ProjectDomainException($"you can't join your own project");
           }
           project.AddContributor(request.Contributor);
           await _projectRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }
}