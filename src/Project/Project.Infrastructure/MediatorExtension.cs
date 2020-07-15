using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.SeedWork;

namespace Project.Infrastructure
{
    public static class MediatorExtension
    {
        /// <summary>
        /// 取出实体内的事件,由mediator推送
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, ProjectContext ctx)
        {
            var domainEntities = ctx.ChangeTracker.Entries<Entity>()
                .Where(x=>x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());
            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();
            foreach (var entry in domainEntities.ToList())
            {
                entry.Entity.ClearDomainEvent();
            }

            var tasks = domainEvents
                .Select(async (domainEvent) => { await mediator.Publish(domainEvent); });

            await Task.WhenAll(tasks);
        }
    }
}