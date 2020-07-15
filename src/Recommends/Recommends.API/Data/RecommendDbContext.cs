using Microsoft.EntityFrameworkCore;
using Recommends.API.Models;

namespace Recommends.API.Data
{
    public class RecommendDbContext:DbContext
    {
        public RecommendDbContext(DbContextOptions<RecommendDbContext> options):base(options)
        {
            
        }

        public DbSet<Recommend> Recommends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Recommend>().ToTable("ProjectRecommends")
                .HasKey(p => p.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}