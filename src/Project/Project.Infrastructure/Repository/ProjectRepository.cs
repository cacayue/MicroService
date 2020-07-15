using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Domain.AggregatesModel;
using Project.Domain.SeedWork;
using ProjectEntity = Project.Domain.AggregatesModel.Project;

namespace Project.Infrastructure.Repository
{
    public class ProjectRepository:IProjectRepository
    {
        private readonly ProjectContext _context;
        public IUnitOfWork UnitOfWork => _context;

        public ProjectRepository(ProjectContext context)
        {
            _context = context;
        }

        public ProjectEntity Add(ProjectEntity project)
        {
            if (project.IsTransient())
            {
               return _context.Projects.Add(project).Entity;
            }
            else
            {
                return project;
            }
        }

        public void Update(ProjectEntity project)
        {
           _context.Projects.Update(project);
        }

        public async Task<ProjectEntity> GetAsync(int id, CancellationToken cancellationToken)
        {
            var result = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Properties)
                .Include(p => p.VisibleRule)
                .Include(p => p.Viewers)
                .Include(p => p.Contributors)
                .SingleOrDefaultAsync(cancellationToken);
            return result;
        }
    }
}