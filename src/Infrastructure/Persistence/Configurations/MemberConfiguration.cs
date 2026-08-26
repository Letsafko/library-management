using Domain.Members;
using Domain.Members.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class MemberConfiguration : EntityTypeBaseConfiguration<Member>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.FirstName)
            .HasColumnName("firstName")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.LastName)
            .HasColumnName("lastName")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(m => m.MembershipType)
            .HasColumnName("membershipType")
            .IsRequired()
            .HasConversion(
                mt => mt.Name,
                name => MembershipType.GetByName(name))
            .HasMaxLength(50);

        builder.HasMany(m => m.Loans)
            .WithOne()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Ignore(m => m.ActiveLoansCount);
        builder.Ignore(m => m.HasReachedLoanLimit);

        builder.HasIndex(m => m.Email)
            .IsUnique();
    }
}
