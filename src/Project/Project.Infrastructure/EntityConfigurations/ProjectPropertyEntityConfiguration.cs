using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfigurations
{
    public class ProjectPropertyEntityConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.ProjectProperty>
    {
        public void Configure(EntityTypeBuilder<Domain.AggregatesModel.ProjectProperty> builder)
        {
            builder.Property(p => p.Key).HasMaxLength(20);
            builder.Property(p => p.Text).HasMaxLength(20);
            builder.Property(p => p.Value).HasMaxLength(20);
            builder.ToTable("ProjectProperties")
                .HasKey(p=>new { p.ProjectId,p.Key ,p.Value });
        }
    }
}