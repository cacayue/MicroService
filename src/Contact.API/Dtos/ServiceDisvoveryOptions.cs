namespace Contact.API.Dtos
{
    public class ServiceDiscoveryOptions
    {
        public string ServiceName { get; set; }
        public string UserServiceName { get; set; }

        public ConsulOptions Consul { get; set; }
    }
}
