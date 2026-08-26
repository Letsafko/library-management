namespace SharedKernel.Primitives;

public interface IValueObject<T, TValue>
{
    TValue Value { get; }
    static abstract Result<T> Create(TValue value);
}