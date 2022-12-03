using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hestia.Core.Web
{
    public static class HttpRequestExtensions
    {
        public static async Task<string> ReadStringAsync(this HttpRequest request,Encoding encoding = null)
        {            
            var media = Utility.GetMediaType(request.ContentType);
            var charset = Core.Utility.GetEncoding(media?.CharSet, encoding) ?? Encoding.UTF8;
            using var sr = new StreamReader(request.Body, charset , true, -1, true);
            var body = await sr.ReadToEndAsync();
            return body;
        }
    }
}
