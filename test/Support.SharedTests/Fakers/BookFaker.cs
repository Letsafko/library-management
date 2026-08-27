using System;
using Bogus;
using Support.SharedTests.Stubs;

namespace Support.SharedTests.Fakers;

public sealed class BookFaker : Faker<BookStub>
{
    public BookFaker(
        int id = 0,
        int bookCopyId = 0,
        string? isbn = null,
        DateTime? createdDatetime = null,
        DateTime? lastModifiedDatetime = null)
    {
        CustomInstantiator(f =>
        {
            var title = f.Lorem.Sentence();
            var author = f.Name.FullName();
            var createdDate = createdDatetime ?? f.Date.Past(2);
            var lastModifiedDate = lastModifiedDatetime ?? f.Date.Between(createdDate, DateTime.UtcNow);

            var book = new BookStub(
                id,
                title,
                author,
                createdDatetime: createdDate,
                lastModifiedDatetime: lastModifiedDate);
            
            book.AddCopy(isbn ?? f.Commerce.Ean13(), createdDate, bookCopyId);
            return book;
        });
    }
}