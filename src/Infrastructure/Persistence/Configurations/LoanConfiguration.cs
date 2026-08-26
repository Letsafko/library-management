using Domain.Books;
using Domain.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class LoanConfiguration : EntityTypeBaseConfiguration<Loan>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.MemberId)
            .HasColumnName("memberId")
            .IsRequired();

        builder.Property(l => l.BookCopyId)
            .HasColumnName("bookCopyId")
            .IsRequired();

        builder.Property(l => l.BorrowedAt)
            .HasColumnName("borrowedAt")
            .IsRequired();

        builder.Property(l => l.DueDate)
            .HasColumnName("dueDate")
            .IsRequired();

        builder.Property(l => l.ReturnedAt)
            .HasColumnName("returnedAt");

        builder.Ignore(l => l.IsReturned);

        builder.HasOne<BookCopy>()
            .WithMany()
            .HasForeignKey(l => l.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_loans_bookCopyId");

        builder.HasIndex(l => l.MemberId).HasDatabaseName("ix_loans_memberId");
        builder.HasIndex(l => l.BookCopyId).HasDatabaseName("ix_loans_bookCopyId");
        builder.HasIndex(l => new { l.BookCopyId, l.ReturnedAt }).HasDatabaseName("ix_loans_bookCopyId_returnedAt");
    }
}