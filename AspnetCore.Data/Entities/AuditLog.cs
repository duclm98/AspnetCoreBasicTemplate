using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace AspnetCore.Data.Entities;

public class AuditLog : BaseEntity
{
    public AuditLogType Type { get; set; } = AuditLogType.Other;
    public string TableName { get; set; }
    public string PrimaryKey { get; set; }
    public string ColumnName { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
}

public enum AuditLogType
{
    [Display(Name = "Khác")] Other = 1,
    [Display(Name = "Xem")] Get,
    [Display(Name = "Thêm")] Create,
    [Display(Name = "Sửa")] Update,
    [Display(Name = "Xóa")] Delete
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.TableName).IsRequired();
        builder.Property(x => x.PrimaryKey).IsRequired();
        builder.Property(x => x.ColumnName).IsRequired(false);
        builder.Property(x => x.OldValues).IsRequired(false);
        builder.Property(x => x.NewValues).IsRequired(false);
    }
}