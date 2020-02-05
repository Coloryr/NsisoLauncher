using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public class HttpRequesterAPI
    {
        /// <summary>
        /// 代理
        /// </summary>
        public static WebProxy Proxy;

        /// <summary>
        /// 客户端
        /// </summary>
        public HttpClient client;
        
        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="Timeout">超时</param>
        public HttpRequesterAPI(TimeSpan Timeout)
        {
            var handler = new HttpClientHandler()
            {
                Proxy = Proxy,
                UseProxy = Proxy == null ? false : true
            };
            client = new HttpClient(handler);
            client.Timeout = Timeout == null ? TimeSpan.FromSeconds(30) : Timeout;
        }

        /// <summary>
        /// 异步Get
        /// </summary>
        /// <param name="uri">网址</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> HttpGetAsync(string uri)
        {
            int a = 0;

            if (uri.Contains("https://"))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            else
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
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
        /// 异步获取字符串
        /// </summary>
        /// <param name="uri">网址</param>
        /// <returns></returns>
        public async Task<string> HttpGetStringAsync(string uri)
        {
            int a = 0;
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

        /// <summary>
        /// 异步Post
        /// </summary>
        /// <param name="uri">网址</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
        {
            return await client.PostAsync(uri, new FormUrlEncodedContent(arg));
        }

        /// <summary>
        /// 异步Post获取字符串
        /// </summary>
        /// <param name="uri">网址</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public async Task<string> HttpPostReadAsStringForString(string uri, Dictionary<string, string> arg)
        {
            var result = await HttpPostAsync(uri, arg);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
