using System;

namespace SharedKernel.Primitives;

public sealed class Result<TValue> : Result
{
	public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failure result can't be accessed.");

	private readonly TValue? _value;

	internal Result(TValue? value, bool isSuccess, ErrorResult error) : base(isSuccess, error)
	{
		_value = value;
	}

	public static implicit operator Result<TValue>(TValue value)
	{
		return value is not null ? Success(value) : Failure<TValue>(ErrorResult.NullValue);
	}

	public static implicit operator Result<TValue>(ErrorResult error)
	{
		return Failure<TValue>(error);
	}
}