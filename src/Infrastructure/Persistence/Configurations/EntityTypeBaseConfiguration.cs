using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Primitives;

namespace Infrastructure.Persistence.Configurations;

public abstract class EntityTypeBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : Entity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(static b => b.CreatedDatetime)
            .HasColumnName("createdOn");
        
        builder.Property(static b => b.LastModifiedDatetime)
            .HasColumnName("lastModifiedOn");

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}