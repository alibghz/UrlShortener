namespace UrlShortener.Services
{
    public class UrlServiceOptions
    {
        public int CodeLength { get; set; } = 6;
        public string BaseUrl { get; set; }
    }
}
