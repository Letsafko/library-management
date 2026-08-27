using System;

namespace SharedKernel.Primitives;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}