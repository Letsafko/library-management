using SharedKernel.Primitives;

namespace Domain.Books;

public static class BookErrors
{
    public static ErrorResult MissingBookTitle => ErrorResult.Problem(
        code: "Book.MissingTitle", 
        description: "Missing book title.");
    
    public static ErrorResult MissingBookAuthor => ErrorResult.Problem(
        code: "Book.AuthorEmpty", 
        description: "Missing book author.");
    
    public static ErrorResult MissingBookIsbn => ErrorResult.Problem(
        code: "Book.IsbnEmpty", 
        description: "Missing book ISBN.");
    
    public static ErrorResult InvalidCopiesCount => ErrorResult.Problem(
        code: "Book.InvalidCopiesCount", 
        description: "Invalid copies count.");
    
    public static ErrorResult InvalidIsbnLength => ErrorResult.Problem(
        code: "ISBN.InvalidLength", 
        description: "ISBN should contain 10 or 13 characters.");
        
    public static ErrorResult AlreadyBorrowed => ErrorResult.Problem(
            code: "BookCopy.AlreadyBorrowed", 
            description: "Copy already borrowed.");
    
    public static ErrorResult CopyNotFound => ErrorResult.Problem(
            code: "BookCopy.CopyNotFound", 
            description: "Book copy not found.");
    
    public static ErrorResult CopyAlreadyReturned => ErrorResult.Problem(
        code: "BookCopy.AlreadyReturned", 
        description: "Book copy has already been returned.");
    
    public static ErrorResult BookNotFound => ErrorResult.NotFound(
        code: "Book.NotFound",
        description: "Book not found.");

    public static ErrorResult InvalidBookId => ErrorResult.Problem(
        code: "Book.InvalidId",
        description: "Book id must be greater than zero.");
}