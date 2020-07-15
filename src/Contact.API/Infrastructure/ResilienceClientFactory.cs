using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience.Http;

namespace Contact.API.Infrastructure
{
    public class ResilienceClientFactory
    {
        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //重试次数
        private readonly int _retryCount;
        //熔断前允许的异常次数
        private readonly int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory(ILogger<ResilienceHttpClient> logger,
            IHttpContextAccessor httpContextAccessor,
            int retryCount,
            int exceptionCountAllowedBeforeBreaking)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
        }

        public ResilienceHttpClient GetResilienceHttpClient()=>
            new ResilienceHttpClient(CreatePolicies, _logger,_httpContextAccessor);

        public AsyncPolicy[] CreatePolicies(string origin)
        {
            return new AsyncPolicy[]
            {
                Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(_retryCount,
                        retryAttempt=>TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),
                        (exception,timespan,retryCount,context) =>
                        {
                            var msg = $"第{retryCount}次重试," +
                                      $"{context.PolicyKey}," +
                                      $"{context.OperationKey}," +
                                      $"原因: {exception}";
                            _logger.LogWarning(msg);
                            _logger.LogError(msg);
                        }),
                Policy.Handle<HttpRequestException>()
                    .CircuitBreakerAsync(_exceptionCountAllowedBeforeBreaking,
                        TimeSpan.FromMinutes(1),
                        (exception,duration) =>
                        {
                            _logger.LogTrace("熔断器开启");
                        },
                        () =>
                        {
                            _logger.LogTrace("熔断器闭合");
                        }),
            };
        }
    }
}