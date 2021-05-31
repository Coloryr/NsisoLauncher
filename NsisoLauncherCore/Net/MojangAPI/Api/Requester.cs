using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Api
{

    /// <summary>
    /// 请求者class, 处理所有的请求.
    /// </summary>
    public static class Requester
    {
        /// <summary>
        /// 定义读取响应和写入请求的编码.
        /// </summary>
        public static Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// 定义验证服务器URL
        /// </summary>
        public static string AuthURL = "https://authserver.mojang.com";

        /// <summary>
        /// NsisoLauncher5目前名称.
        /// </summary>
        public readonly static string Name = "NsisoLauncher";

        /// <summary>
        /// NsisoLauncher5目前版本号.
        /// </summary>
        public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// 代表请求者实例的UUID.
        /// </summary>
        public static string ClientToken
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_clientToken))
                {
                    _clientToken = Guid.NewGuid().ToString("N");
                }
                return _clientToken;
            }
            set { _clientToken = value; }
        }
        private static string _clientToken;

        /// <summary>
        /// 向给定endpoint发送GET请求.
        /// </summary>
        /// <typeparam name="T">返回响应的类型</typeparam>
        internal async static Task<Response> Get<T>(IEndpoint<T> endpoint, bool authenticate = false)
        {
            if (endpoint == null)
                throw new ArgumentNullException("Endpoint", "Endpoint should not be null.");

            HttpResponseMessage httpResponse = null;
            Error error = null;
            string rawMessage = null;

            try
            {
                HttpRequestMessage requestMessage = new();
                // 如果给予了令牌
                if (authenticate && endpoint.Arguments.Count > 0)
                {
                    //application/x-www-form-urlencoded
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.Arguments[0]);
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue(Name, Version));
                }

                requestMessage.RequestUri = endpoint.Address;
                requestMessage.Method = HttpMethod.Get;
                httpResponse = await HttpRequesterAPI.client.SendAsync(requestMessage);
                rawMessage = await httpResponse.Content.ReadAsStringAsync();
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                error = new()
                {
                    ErrorMessage = ex.Message,
                    ErrorTag = ex.Message,
                    Exception = ex
                };
            }
            return new()
            {
                Code = httpResponse.StatusCode,
                RawMessage = rawMessage,
                IsSuccess = httpResponse.IsSuccessStatusCode && (
                            httpResponse.StatusCode == HttpStatusCode.Accepted ||
                            httpResponse.StatusCode == HttpStatusCode.Continue ||
                            httpResponse.StatusCode == HttpStatusCode.Created ||
                            httpResponse.StatusCode == HttpStatusCode.Found ||
                            httpResponse.StatusCode == HttpStatusCode.OK ||
                            httpResponse.StatusCode == HttpStatusCode.PartialContent ||
                            httpResponse.StatusCode == HttpStatusCode.NoContent) &&
                            error == null,
                Error = error
            };

        }

        /// <summary>
        /// 向给定endpoint发送POST请求.
        /// </summary>
        /// <typeparam name="T">返回响应的类型</typeparam>
        internal async static Task<Response> Post<T>(IEndpoint<T> endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException("Endpoint", "Endpoint should not be null.");

            if (endpoint.PostContent == null)
                throw new ArgumentNullException("PostContent", "PostContent should not be null.");

            HttpResponseMessage httpResponse = null;
            Error error = null;
            string rawMessage = null;

            try
            {
                StringContent contents = new(endpoint.PostContent, Encoding, "application/json");
                httpResponse = await HttpRequesterAPI.client.PostAsync(endpoint.Address, contents);
                if (httpResponse == null || httpResponse.Content == null)
                {
                    return new()
                    {
                        Code = httpResponse.StatusCode,
                        RawMessage = rawMessage,
                        IsSuccess = false,
                        Error = error
                    };
                }
                rawMessage = await httpResponse.Content.ReadAsStringAsync();
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (TaskCanceledException ex)
            {
                error = new()
                {
                    ErrorMessage = "TimeOut",
                    ErrorTag = ex.Message,
                    Exception = ex
                };
                return new()
                {
                    Code = HttpStatusCode.GatewayTimeout,
                    RawMessage = rawMessage,
                    IsSuccess = false,
                    Error = error
                };
            }
            catch (Exception ex)
            {
                error = new()
                {
                    ErrorMessage = ex.Message,
                    ErrorTag = ex.Message,
                    Exception = ex
                };
                if (httpResponse == null)
                {
                    return new()
                    {
                        Code = HttpStatusCode.ServiceUnavailable,
                        RawMessage = rawMessage,
                        IsSuccess = false,
                        Error = error
                    };
                }
                return new()
                {
                    Code = httpResponse.StatusCode,
                    RawMessage = rawMessage,
                    IsSuccess = false,
                    Error = error
                };
            }

            return new()
            {
                Code = httpResponse.StatusCode,
                RawMessage = rawMessage,
                IsSuccess = httpResponse.IsSuccessStatusCode && (
                            httpResponse.StatusCode == HttpStatusCode.Accepted ||
                            httpResponse.StatusCode == HttpStatusCode.Continue ||
                            httpResponse.StatusCode == HttpStatusCode.Created ||
                            httpResponse.StatusCode == HttpStatusCode.Found ||
                            httpResponse.StatusCode == HttpStatusCode.OK ||
                            httpResponse.StatusCode == HttpStatusCode.PartialContent) &&
                            error == null,
                Error = error
            };
        }

        /// <summary>
        /// 向给定endpoint发送POST请求.
        /// </summary>
        internal async static Task<Response> Post<T>(IEndpoint<T> endpoint, Dictionary<string, string> toEncode)
        {
            if (endpoint == null)
                throw new ArgumentNullException("Endpoint", "Endpoint should not be null.");

            if (toEncode == null || toEncode.Count < 1)
                throw new ArgumentNullException("PostContent", "PostContent should not be null.");


            HttpResponseMessage httpResponse = null;
            Error error = null;
            string rawMessage = null;

            try
            {
                HttpRequestMessage requestMessage = new();
                //application/x-www-form-urlencoded
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.Arguments[0]);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue(Name, Version));

                requestMessage.RequestUri = endpoint.Address;
                requestMessage.Method = HttpMethod.Post;
                using var content = new FormUrlEncodedContent(toEncode);
                requestMessage.Content = content;
                httpResponse = await HttpRequesterAPI.client.SendAsync(requestMessage);
                rawMessage = await httpResponse.Content.ReadAsStringAsync();
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    JObject err = JObject.Parse(rawMessage);
                    error = new()
                    {
                        ErrorMessage = err["errorMessage"].ToObject<string>(),
                        ErrorTag = err["error"].ToObject<string>(),
                        Exception = ex
                    };
                }
                else
                {
                    error = new()
                    {
                        ErrorMessage = ex.Message,
                        ErrorTag = ex.GetBaseException().Message,
                        Exception = ex
                    };
                }

            }

            return new()
            {
                Code = httpResponse.StatusCode,
                RawMessage = rawMessage,
                IsSuccess = httpResponse.IsSuccessStatusCode && (
                            httpResponse.StatusCode == HttpStatusCode.Accepted ||
                            httpResponse.StatusCode == HttpStatusCode.Continue ||
                            httpResponse.StatusCode == HttpStatusCode.Created ||
                            httpResponse.StatusCode == HttpStatusCode.Found ||
                            httpResponse.StatusCode == HttpStatusCode.OK ||
                            httpResponse.StatusCode == HttpStatusCode.PartialContent) &&
                            error == null,
                Error = error
            };
        }

        /// <summary>
        /// 向给定endpoint发送一个PUT请求.
        /// </summary>
        internal async static Task<Response> Put<T>(IEndpoint<T> endpoint, FileInfo file)
        {
            if (endpoint == null)
                throw new ArgumentNullException("Endpoint", "Endpoint should not be null.");

            if (file == null)
                throw new ArgumentNullException("Skin", "No file given.");

            if (!file.Exists)
                throw new ArgumentException("Given file does not exist.");


            HttpResponseMessage httpResponse = null;
            Error error = null;
            string rawMessage = null;

            try
            {
                HttpRequestMessage requestMessage = new();
                //application/x-www-form-urlencoded
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.Arguments[0]);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue(Name, Version));

                using var contents = new MultipartFormDataContent();

                contents.Add(new StringContent(bool.Parse(endpoint.Arguments[1]) == true ? "true" : "false"), "model");
                contents.Add(new ByteArrayContent(File.ReadAllBytes(file.FullName)), "file", file.Name);

                requestMessage.Method = HttpMethod.Put;
                requestMessage.Content = contents;
                requestMessage.RequestUri = endpoint.Address;

                httpResponse = await HttpRequesterAPI.client.SendAsync(requestMessage);
                rawMessage = await httpResponse.Content.ReadAsStringAsync();
                httpResponse.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    JObject err = JObject.Parse(rawMessage);
                    error = new()
                    {
                        ErrorMessage = err["errorMessage"].ToObject<string>(),
                        ErrorTag = err["error"].ToObject<string>(),
                        Exception = ex
                    };
                }
                else
                {
                    error = new()
                    {
                        ErrorMessage = ex.Message,
                        ErrorTag = ex.GetBaseException().Message,
                        Exception = ex
                    };
                }

            }

            return new()
            {
                Code = httpResponse.StatusCode,
                RawMessage = rawMessage,
                IsSuccess = httpResponse.IsSuccessStatusCode && (
                            httpResponse.StatusCode == HttpStatusCode.Accepted ||
                            httpResponse.StatusCode == HttpStatusCode.Continue ||
                            httpResponse.StatusCode == HttpStatusCode.Created ||
                            httpResponse.StatusCode == HttpStatusCode.Found ||
                            httpResponse.StatusCode == HttpStatusCode.OK ||
                            httpResponse.StatusCode == HttpStatusCode.PartialContent) &&
                            error == null,
                Error = error
            };
        }

        /// <summary>
        /// 向给定endpoint发送DELETE请求.
        /// </summary>
        internal async static Task<Response> Delete<T>(IEndpoint<T> endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException("Endpoint", "Endpoint should not be null.");


            HttpResponseMessage httpResponse = null;
            Error error = null;
            string rawMessage = null;

            try
            {
                HttpRequestMessage requestMessage = new();
                //application/x-www-form-urlencoded
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.Arguments[0]);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue(Name, Version));
                requestMessage.RequestUri = endpoint.Address;
                requestMessage.Method = HttpMethod.Delete;

                httpResponse = await HttpRequesterAPI.client.SendAsync(requestMessage);
                rawMessage = await httpResponse.Content.ReadAsStringAsync();
                httpResponse.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    JObject err = JObject.Parse(rawMessage);
                    error = new()
                    {
                        ErrorMessage = err["errorMessage"].ToObject<string>(),
                        ErrorTag = err["error"].ToObject<string>(),
                        Exception = ex
                    };
                }
                else
                {
                    error = new()
                    {
                        ErrorMessage = ex.Message,
                        ErrorTag = ex.GetBaseException().Message,
                        Exception = ex
                    };
                }

            }

            return new()
            {
                Code = httpResponse.StatusCode,
                RawMessage = rawMessage,
                IsSuccess = httpResponse.IsSuccessStatusCode && (
                            httpResponse.StatusCode == HttpStatusCode.Accepted ||
                            httpResponse.StatusCode == HttpStatusCode.Continue ||
                            httpResponse.StatusCode == HttpStatusCode.Created ||
                            httpResponse.StatusCode == HttpStatusCode.Found ||
                            httpResponse.StatusCode == HttpStatusCode.OK ||
                            httpResponse.StatusCode == HttpStatusCode.PartialContent) &&
                            error == null,
                Error = error
            };
        }
    }
}
