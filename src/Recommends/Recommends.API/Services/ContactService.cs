using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recommends.API.Dtos;
using Resilience.Http;

namespace Recommends.API.Services
{
    public class ContactService:IContactService
    {
        private readonly IHttpClient _httpClient;
        private readonly string _contactServiceUrl;
        private readonly ILogger<UserService> _logger;

        public ContactService(IHttpClient httpClient, IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> options, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var hostEntries = dnsQuery
                .ResolveService("service.consul", options.Value.ContactServiceName);
            var addressList = hostEntries.First().AddressList;
            var host = addressList.Any() ? addressList.First().ToString() : hostEntries.First().HostName;
            var port = hostEntries.First().Port;
            _contactServiceUrl = $"http://{host}:{port}";
        }
        public async Task<List<Contact>> GetContactsByUserId(int userId)
        {
            _logger.LogTrace($"Find GetContactsByUserId by Id:{userId}");
            var form = new Dictionary<string, int> { { "userId", userId } };
            try
            {
                var response = await _httpClient.GetAsync(_contactServiceUrl + "/api/contacts/" + userId);
                if (response.StatusCode != HttpStatusCode.OK) return null;
                var userInfoStr = await response.Content.ReadAsStringAsync();
                var contacts = JsonConvert.DeserializeObject<List<Contact>>(userInfoStr);
                _logger.LogTrace($"Completed GetContactsByUserId with userId:{userId}");
                return contacts;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetContactsByUserId 在重试之后失败," + e.Message + e.StackTrace);
                throw e;
            }
        }
    }
}