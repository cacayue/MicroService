using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Resilience.Http
{
    public class ResilienceHttpClient:IHttpClient
    {
        private readonly HttpClient _httpClient;
        //根据uri 创建policy
        private readonly Func<string, IEnumerable<AsyncPolicy>> _policyCreator;
        //把policy打包组合成policy wrappers,进行本地缓存
        private readonly ConcurrentDictionary<string, AsyncPolicyWrap> _policyWrappers = new ConcurrentDictionary<string, AsyncPolicyWrap>();
        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(Func<string, IEnumerable<AsyncPolicy>> policyCreator,
            ILogger<ResilienceHttpClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = new HttpClient();
            _policyCreator = policyCreator;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string uri, T item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Post, uri,item);
            return await DoPostAsync(HttpMethod.Post, uri, func, authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PostAsync(string uri, Dictionary<string,string> form, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Post, uri, form);
            return await DoPostAsync(HttpMethod.Post, uri, func, authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> GetAsync(string uri, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
           return await DoGetAsync(HttpMethod.Get,uri, authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string uri, T item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Post, uri, item);
            return await DoPostAsync(HttpMethod.Post, uri, func, authorizationToken, requestId, authorizationMethod);
        }

        private Task<HttpResponseMessage> DoPostAsync(HttpMethod method,string uri, Func<HttpRequestMessage> requestFunc, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                _logger.LogError("Value must be either post or put", nameof(method));
                throw new ArgumentException("Value must be either post or put",nameof(method));
            }

            var origin = GetOriginFromUri(uri);
            return HttpInvoker(origin, async (context) =>
            {
                var requestMessage = requestFunc();

                SetAuthorizationHandler(requestMessage);
               
                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }

        private Task<HttpResponseMessage> DoGetAsync(HttpMethod method, string uri, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Get)
            {
                _logger.LogError("Value must be either get", nameof(method));
                throw new ArgumentException("Value must be either get", nameof(method));
            }

            var origin = GetOriginFromUri(uri);
            return HttpInvoker(origin, async (context) =>
            {
                var requestMessage = new HttpRequestMessage(method, uri);

                SetAuthorizationHandler(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }

        private HttpRequestMessage CreateHttpRequestMessage<T>(HttpMethod method,string uri, T item)
        {
            var requestMessage = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json")
            };
            return requestMessage;
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string uri, Dictionary<string,string> form)
        {
            var requestMessage = new HttpRequestMessage(method, uri) {Content = new FormUrlEncodedContent(form) };
            return requestMessage;
        }

        private async Task<T> HttpInvoker<T>(string origin, Func<Context,Task<T>> action)
        {

            var normalizeOrigin = NormalizeOrigin(origin);
            if (!_policyWrappers.TryGetValue(normalizeOrigin, out AsyncPolicyWrap policyWrap))
            {
                if (_policyCreator == null)
                {
                    _logger.LogError("policyCreator is null", nameof(_policyCreator));
                    throw new ArgumentException("policyCreator is null", nameof(_policyCreator));
                }

                policyWrap = Policy.WrapAsync(_policyCreator(normalizeOrigin).ToArray());
                _policyWrappers.TryAdd(normalizeOrigin, policyWrap);
            }

            return await policyWrap.ExecuteAsync(action,new Context(normalizeOrigin));
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private static string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }

        private void SetAuthorizationHandler(HttpRequestMessage requestMessage)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return;
            }
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization",new List<string>(){authorizationHeader});
            }
        }
    }
}