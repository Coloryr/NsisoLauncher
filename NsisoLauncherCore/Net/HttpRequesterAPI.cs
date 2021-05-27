using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public class HttpRequesterAPI
    {
        private static WebProxy _Proxy;
        /// <summary>
        /// 客户端
        /// </summary>
        public static HttpClient client { get; private set; }
        /// <summary>
        /// 代理
        /// </summary>
        public static WebProxy Proxy
        {
            get
            {
                return _Proxy;
            }
            set
            {
                _Proxy = value;
                var handler = new HttpClientHandler()
                {
                    Proxy = _Proxy,
                    UseProxy = _Proxy != null
                };
                client?.Dispose();
                client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(20)
                };
            }
        }

        static HttpRequesterAPI()
        {
            var handler = new HttpClientHandler()
            {
                Proxy = _Proxy,
                UseProxy = _Proxy != null
            };
            client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
        }
        /// <summary>
        /// 异步Get
        /// </summary>
        /// <param name="uri">网址</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpGetAsync(string uri)
        {
            int a = 0;
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

        /// <summary>
        /// 异步Get
        /// </summary>
        /// <param name="uri">网址</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpGetAsync(string uri, CancellationToken cancelToken)
        {
            int a = 0;
            while (a < 5)
            {
                try
                {
                    var b = await client.GetAsync(uri, cancelToken);
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

        /// <summary>
        /// 异步Get
        /// </summary>
        /// <param name="uri">网址</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpGetAsync(string uri, CancellationToken cancelToken, HttpCompletionOption option)
        {
            int a = 0;
            while (a < 5)
            {
                try
                {
                    var b = await client.GetAsync(uri, option, cancelToken);
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

        /// <summary>
        /// 异步获取字符串
        /// </summary>
        /// <param name="uri">网址</param>
        /// <returns></returns>
        public static async Task<string> HttpGetStringAsync(string uri)
        {
            int a = 0;
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

        /// <summary>
        /// 异步Post
        /// </summary>
        /// <param name="uri">网址</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
        {
            return await client.PostAsync(uri, new FormUrlEncodedContent(arg));
        }

        /// <summary>
        /// 异步Post获取字符串
        /// </summary>
        /// <param name="uri">网址</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static async Task<string> HttpPostReadAsStringForString(string uri, Dictionary<string, string> arg)
        {
            var result = await HttpPostAsync(uri, arg);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
