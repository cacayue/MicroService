using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommends.API.Dtos
{
    public class ServiceDiscoveryOptions
    {
        public string ServiceName { get; set; }
        public string UserServiceName { get; set; }
        public string ContactServiceName { get; set; }

        public ConsulOptions Consul { get; set; }
    }
}
