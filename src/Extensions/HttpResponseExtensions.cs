using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hestia.Core.Web
{
    public static class HttpResponseExtensions
    {
        public static async Task ResponseAsync(this HttpResponse response, int code, string body = null, string type = null, Encoding encoding = null)
        {
            response.StatusCode = code;
            if (!string.IsNullOrEmpty(type)) { response.ContentType = type; }
            if (!string.IsNullOrEmpty(body))
            {
                var media = Utility.GetMediaType(response.ContentType);
                var charset = Core.Utility.GetEncoding(media?.CharSet, encoding) ?? Encoding.UTF8;
                await response.WriteAsync(body, charset);
            }
        }
    }
}
