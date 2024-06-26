using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspnetCore.Data.Entities;

public class UserCredential : BaseEntity
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string RefreshToken { get; set; }
}

public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("UserCredentials");
        builder.Property(x => x.Username).IsRequired();
        builder.Property(x => x.Password).IsRequired();
        builder.Property(x => x.RefreshToken).IsRequired(false);
    }
}