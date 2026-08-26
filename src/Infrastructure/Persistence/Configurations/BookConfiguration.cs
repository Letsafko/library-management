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

        builder.Property(static b => b.Isbn)
            .HasColumnName("isbn")
            .IsRequired()
            .HasMaxLength(13)
            .HasConversion(static x => x.Value, static x => Isbn.Create(x).Value);

        builder.HasMany(b => b.BookCopies)
            .WithOne(c => c.Book)
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(b => b.AvailableCopiesCount);
        builder.Ignore(b => b.TotalCopiesCount);

        builder.HasIndex(b => b.Isbn).IsUnique();

    }
}
