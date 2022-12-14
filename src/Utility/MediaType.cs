using System.Net.Http.Headers;

namespace Hestia.Core.Web
{
    public static partial class Utility
    {
        public static class MediaType
        {
            public const string Text = "text/plain";
            public const string Json = "application/json";
            public const string Xml = "application/xml";
        }

        public static MediaTypeHeaderValue GetMediaType(string type)
        {
            if(string.IsNullOrEmpty(type)) { return null; }
            if (MediaTypeHeaderValue.TryParse(type, out var media))
            {
                return media;
            }
            return null;
        }
    }
}
