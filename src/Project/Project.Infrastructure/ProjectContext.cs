using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Project.Domain.SeedWork;
using Project.Infrastructure.EntityConfigurations;

namespace Project.Infrastructure
{
    public class ProjectContext:DbContext,IUnitOfWork
    {
        private readonly IMediator _mediator;

        public ProjectContext(IMediator mediator, DbContextOptions<ProjectContext> options):base(options)
        {
            _mediator = mediator;
        }

        public DbSet<Domain.AggregatesModel.Project> Projects { get; set; }
        public DbSet<Domain.AggregatesModel.ProjectContributor> Contributors { get; set; }
        public DbSet<Domain.AggregatesModel.ProjectViewer> Viewers { get; set; }
        public DbSet<Domain.AggregatesModel.ProjectProperty> Properties { get; set; }
        public DbSet<Domain.AggregatesModel.ProjectVisibleRule> VisibleRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectContributorEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectViewerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectVisibleRuleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectPropertyEntityConfiguration());
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _mediator.DispatchDomainEventsAsync(this);
            await base.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}