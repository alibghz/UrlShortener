using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.Services.Extensions
{
    public static partial class Extensions
    {
        public static string GetBaseUrl(this HttpRequest request)
        {
            var port = request.Scheme.ToLower() == "http" ? (request.Host.Port != 80 ? $":{request.Host.Port}" : "") : (request.Host.Port != 443 ? $":{request.Host.Port}" : "");
            return $"{request.Scheme}://{request.Host.Host.TrimEnd(new char[] { ':', '/' })}{port.TrimEnd(new char[] { ':' })}/";
        }
    }
}
