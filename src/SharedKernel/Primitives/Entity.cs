using System;

namespace SharedKernel.Primitives;

public abstract class Entity(DateTime createdDatetime, DateTime lastModifiedDatetime)
{
    public DateTime LastModifiedDatetime { get; private set; } = lastModifiedDatetime;
    public DateTime CreatedDatetime { get; } = createdDatetime;
}