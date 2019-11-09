using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace UrlShortener.Domain.Entities
{
    [ExcludeFromCodeCoverage]
    public class ShortenedUrl : BaseEntity<Guid>
    {
        [Required]
        [MinLength(3)]
        [MaxLength(128)]
        public string Code { get; set; }

        [Required]
        [MinLength(12)]
        public string Url { get; set; }
    }
}
