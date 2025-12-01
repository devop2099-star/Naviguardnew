
namespace Naviguard.Domain.Entities
{
    public class ProxyInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // Devuelve la dirección ip:puerto
        public string GetProxyString() => $"{Host}:{Port}";
    }
}
