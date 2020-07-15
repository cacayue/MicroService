using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contact.API.Dtos;
using Contact.API.Entity.Dtos;
using DnsClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience.Http;


namespace Contact.API.Services
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

        public async Task<BaseUserInfo> GetBaseUserInfoAsync(int userId)
        {
            _logger.LogTrace($"Find userInfo by Id:{userId}");
            var form = new Dictionary<string, int> { { "userId", userId } };
            try
            {
                  
                var response = await _httpClient.GetAsync(_userServiceUrl + "/api/users/baseinfo/" + userId
                    );
                if (response.StatusCode != HttpStatusCode.OK) return null;
                var userInfoStr = await response.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<BaseUserInfo>(userInfoStr);
                _logger.LogTrace($"Completed GetBaseUserInfoAsync with userId:{userInfo.Id}");
                return userInfo;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetBaseUserInfoAsync 在重试之后失败," + e.Message + e.StackTrace);
                throw e;
            }
        }
    }
}