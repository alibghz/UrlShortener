using System;
using System.Diagnostics.CodeAnalysis;

namespace UrlShortener.Domain.Entities
{
    [ExcludeFromCodeCoverage]
    public class BaseEntity<T> : IBaseEntity
    {
        public T Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
