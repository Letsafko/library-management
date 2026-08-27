using System;

namespace SharedKernel.Primitives;

public abstract class Entity<TId>(
    DateTime createdDatetime,
    DateTime lastModifiedDatetime) : Entity(createdDatetime, lastModifiedDatetime)
{
    public TId Id { get; protected set; } = default!;
}