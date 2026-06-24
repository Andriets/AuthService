using AuthService.Web.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Web.Infrastructure.Data.Configurations;

public class AuthenticationProviderConfiguration : IEntityTypeConfiguration<AuthenticationProvider>
{
    public void Configure(EntityTypeBuilder<AuthenticationProvider> builder)
    {
        builder.ToTable("authentication_providers");

        builder.HasKey(ap => ap.Id);
        builder.Property(ap => ap.Id).HasColumnName("id");

        builder.Property(ap => ap.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(ap => ap.Name)
            .HasDatabaseName("uq_authentication_providers_name")
            .IsUnique();
    }
}
