using System;

namespace SharedKernel.Primitives;

public abstract class Entity(DateTimeOffset createdDatetime, DateTimeOffset lastModifiedDatetime)
{
    public DateTimeOffset LastModifiedDatetime { get; private set; } = lastModifiedDatetime;
    public DateTimeOffset CreatedDatetime { get; } = createdDatetime;
}