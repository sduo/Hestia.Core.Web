using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Hestia.Core.Cache
{
    public sealed class  JsonStore<T>(string url, IServiceProvider services, TimeSpan timeout) : HttpStore<T>(url, services, timeout, Deserializer)
    {
        internal static T[] Deserializer(string json) => Utility.FromJson<T[]>(json);
    }

    public class HttpStore<T>(string url,IServiceProvider services, TimeSpan timeout, Func<string, T[]> deserializer) : Store<T, string>(timeout)
    {
        public IReadOnlySet<string> ETags { get; private set; } = null;

        private static IReadOnlySet<string> GetHeaderValue(HttpHeaders headers, string key)
        {
            return (headers?.TryGetValues(key, out var values) == true) ? new HashSet<string>(values, StringComparer.OrdinalIgnoreCase) : null;
        }

        private static async Task<bool> RequestETagAsync(IHttpClientFactory hf,string url, IReadOnlySet<string> etags)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var http = hf.CreateClient();            
            using var response = (await http.SendAsync(request)).EnsureSuccessStatusCode();
            return GetHeaderValue(response.Headers, "ETag")?.Overlaps(etags) ?? false;
        }

        private static async Task<(IReadOnlySet<string>, string)> RequestDataAsync(IHttpClientFactory hf, string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var http = hf.CreateClient();
            using var response = (await http.SendAsync(request)).EnsureSuccessStatusCode();
            var etags = GetHeaderValue(response.Headers, "ETag");
            var body = await response.Content.ReadAsStringAsync();
            return (etags, body);
        }

        protected override async Task LoadAsync(bool reload)
        {
            var hf = services?.GetService<IHttpClientFactory>() ?? throw new NullReferenceException(nameof(IHttpClientFactory));
            if (!reload && (ETags?.Count > 0) && !string.IsNullOrEmpty(RawData) && store is not null)
            {
                if (await RequestETagAsync(hf, url, ETags))
                {
                    UpdateUtc = DateTime.UtcNow;
                    return;
                }
            }
            (var etags,var body) = await RequestDataAsync(hf, url);
            var data = deserializer?.Invoke(body);
            if (data is null) { return; }
            ETags = etags;
            RawData = body;            
            store = data;
        }
    }
}
