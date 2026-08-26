using SharedKernel.Primitives;

namespace Domain.Books;

internal static class BookErrors
{
    internal static ErrorResult MissingBookTitle => ErrorResult.Problem(
        code: "Book.MissingTitle", 
        description: "Missing book title.");
    
    internal static ErrorResult MissingBookAuthor => ErrorResult.Problem(
        code: "Book.AuthorEmpty", 
        description: "Missing book author.");
    
    internal static ErrorResult InvalidCopiesCount => ErrorResult.Problem(
        code: "Book.InvalidCopiesCount", 
        description: "Invalid copies count.");
    
    internal static ErrorResult InvalidIsbnLength => ErrorResult.Problem(
        code: "ISBN.InvalidLength", 
        description: "ISBN should contain 10 or 13 characters.");
        
    internal static ErrorResult AlreadyBorrowed => ErrorResult.Problem(
            code: "BookCopy.AlreadyBorrowed", 
            description: "Copy already borrowed.");
    
    internal static ErrorResult CopyNotFound => ErrorResult.Problem(
            code: "BookCopy.CopyNotFound", 
            description: "Book copy not found.");
    
    internal static ErrorResult CopyAlreadyReturned => ErrorResult.Problem(
        code: "BookCopy.AlreadyReturned", 
        description: "Book copy has already been returned.");
}