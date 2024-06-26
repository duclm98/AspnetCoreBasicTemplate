using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspnetCore.Data.Entities;

public class BaseEntity
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int? CreatedUserId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? ModifiedUserId { get; set; }
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public int? DeletedUserId { get; set; }
    public DateTime? DeletedDate { get; set; }
}

public abstract class BaseEntityConfiguration : IEntityTypeConfiguration<BaseEntity>
{
    public void Configure(EntityTypeBuilder<BaseEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedUserId).IsRequired(false);
        builder.Property(x => x.CreatedDate).IsRequired().HasDefaultValue(DateTime.Now);
        builder.Property(x => x.ModifiedUserId).IsRequired(false);
        builder.Property(x => x.ModifiedDate).IsRequired().HasDefaultValue(DateTime.Now);
        builder.Property(x => x.DeletedUserId).IsRequired(false);
        builder.Property(x => x.DeletedDate).IsRequired(false);
    }
}