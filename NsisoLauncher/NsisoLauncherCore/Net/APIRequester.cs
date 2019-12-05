using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public static class APIRequester
    {
        public async static Task<HttpResponseMessage> HttpGetAsync(string uri)
        {
            int a = 0;
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                while (a < 5)
                {
                    try
                    {
                        var b = await client.GetAsync(uri);
                        return b;
                    }
                    catch
                    {
                        client.CancelPendingRequests();
                        a++;
                    }
                }
                return null;
            }
        }

        public async static Task<string> HttpGetStringAsync(string uri)
        {
            int a = 0;
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                while (a < 5)
                {
                    try
                    {
                        var b = await client.GetStringAsync(uri);
                        return b;
                    }
                    catch
                    {
                        client.CancelPendingRequests();
                        a++;
                    }
                }
                return null;
            }
        }

        public async static Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.PostAsync(uri, new FormUrlEncodedContent(arg));
            }
        }

        public async static Task<string> HttpPostReadAsStringForString(string uri, Dictionary<string, string> arg)
        {
            var result = await HttpPostAsync(uri, arg);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
