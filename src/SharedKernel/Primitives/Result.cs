using System;

namespace SharedKernel.Primitives;

public class Result
{
	public bool IsSuccess { get; }

	public ErrorResult Error { get; }

	protected Result(bool isSuccess, ErrorResult error)
	{
		if((isSuccess && error != ErrorResult.None) || (!isSuccess && error == ErrorResult.None))
		{
			throw new ArgumentException("A successful result cannot contain an error", nameof(error));
		}

		IsSuccess = isSuccess;
		Error = error;
	}

	public static Result Success()
	{
		return new Result(true, ErrorResult.None);
	}

	protected static Result<TValue> Success<TValue>(TValue value)
	{
		return new Result<TValue>(value, true, ErrorResult.None);
	}

	protected static Result<TValue> Failure<TValue>(ErrorResult error)
	{
		return new Result<TValue>(default, false, error);
	}

	public static implicit operator Result(ErrorResult error)
	{
		return new Result(false, error);
	}
}
