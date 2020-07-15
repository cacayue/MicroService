using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience.Http;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public class UserService: IUserService
    {
        private readonly IHttpClient _httpClient;
        private readonly string _userServiceUrl;
        private readonly ILogger<UserService> _logger;

        public UserService(IHttpClient httpClient, IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> options, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var hostEntries = dnsQuery
                .ResolveService("service.consul", options.Value.UserServiceName);
            var addressList = hostEntries.First().AddressList;
            var host = addressList.Any()?addressList.First().ToString(): hostEntries.First().HostName;
            var port = hostEntries.First().Port;
            _userServiceUrl = $"http://{host}:{port}";
        }

        public async Task<UserInfo> CheckOrCreate(string phone)
        {
            _logger.LogTrace($"Enter into CheckOrCreate:{phone}");
            var form = new Dictionary<string,string>{{ "phone", phone } };
            try
            {
                var response = await _httpClient.PostAsync(_userServiceUrl + "/api/users/check-or-create",
                    form);
                if (response.StatusCode != HttpStatusCode.OK) return null;
                var userInfoStr = await response.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoStr);
                _logger.LogTrace($"Completed CheckOrCreate with userId:{userInfo.Id}");
                return userInfo;
            }
            catch (Exception e)
            {
                _logger.LogError($"CheckOrCreate 在重试之后失败,"+e.Message + e.StackTrace);
                throw e;
            }
        }
    }
}