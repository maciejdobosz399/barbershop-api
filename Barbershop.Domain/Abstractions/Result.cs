using Barbershop.Domain.Errors;
using System.Diagnostics.CodeAnalysis;

namespace Barbershop.Domain.Abstractions;

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsFailure => !IsSuccess;
    public static Result Success() => new Result(true, null);
    public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(true, null, value);
    public static Result Failure(Error error) => new Result(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(false, error, default);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;
    public Result(bool isSuccess, Error? error, TValue? value) : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value
    {
        get
        {
            if (IsFailure || _value == null)
            {
                throw new InvalidOperationException("Cannot access the value of a failed result.");
            }
            return _value;
        }
    }

    public static implicit operator Result<TValue>(TValue value) => value is not null ? Success(value) : Failure<TValue>(DomainErrors.NullValue);
}
