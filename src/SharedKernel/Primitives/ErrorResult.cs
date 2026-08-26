namespace SharedKernel.Primitives;

public record ErrorResult
{
	public static readonly ErrorResult None = new(string.Empty, string.Empty, ErrorType.None);
	public static readonly ErrorResult NullValue = new("General.Null", "Null value was provided", ErrorType.Failure);
	protected ErrorResult(string code, string? description, ErrorType type)
	{
		Code = code;
		Type = type;
		Description = description;
	}

	public string Code { get; }

	public string? Description { get; }

	public ErrorType Type { get; }

	public static ErrorResult Failure(string code, string? description = null)
	{
		return new ErrorResult(code, description, ErrorType.Failure);
	}

	public static ErrorResult NotFound(string code, string? description = null)
	{
		return new ErrorResult(code, description, ErrorType.NotFound);
	}

	public static ErrorResult Problem(string code, string? description = null)
	{
		return new ErrorResult(code, description, ErrorType.Problem);
	}

	public static ErrorResult Conflict(string code, string? description = null)
	{
		return new ErrorResult(code, description, ErrorType.Conflict);
	}
}
