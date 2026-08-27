using System;
using Domain.Books;

namespace Support.SharedTests.Stubs;

public sealed class BookStub(
    int id,
    string title,
    string author,
    DateTime createdDatetime,
    DateTime lastModifiedDatetime) : Book(title, author, createdDatetime, lastModifiedDatetime)
{
    public new int Id { get; } = id;
}