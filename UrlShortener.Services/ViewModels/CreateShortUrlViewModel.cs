using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Services.ViewModels
{
    public class CreateShortUrlViewModel
    {
        [Required]
        [DataType(DataType.Url)]
        public string Url { get; set; }
    }
}
