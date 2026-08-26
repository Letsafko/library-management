using Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BookCopyConfiguration : EntityTypeBaseConfiguration<BookCopy>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ToTable("bookCopies");

        builder.HasKey(bc => bc.Id).HasName("pk_bookCopies");

        builder.Property(bc => bc.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(bc => bc.IsAvailable)
            .HasColumnName("isAvailable")
            .IsRequired();

        builder.HasOne(bc => bc.Book)
            .WithMany(b => b.BookCopies)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
