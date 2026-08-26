using Domain.Books;
using Domain.Books.ValueObjects;
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
        
        builder.Property(static b => b.Isbn)
            .HasColumnName("isbn")
            .IsRequired()
            .HasMaxLength(13)
            .HasConversion(static x => x.Value, static x => Isbn.Create(x).Value);
        
        builder.HasIndex(b => b.Isbn).IsUnique();
    }
}
