using Domain.Books;
using Domain.Books.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BookConfiguration : EntityTypeBaseConfiguration<Book>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");

        builder.HasKey(b => b.Id).HasName("pk_books");

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(b => b.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.Author)
            .HasColumnName("author")
            .IsRequired()
            .HasMaxLength(300);

        builder.HasMany(b => b.BookCopies)
            .WithOne(c => c.Book)
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(b => b.AvailableCopiesCount);
        builder.Ignore(b => b.TotalCopiesCount);
    }
}
