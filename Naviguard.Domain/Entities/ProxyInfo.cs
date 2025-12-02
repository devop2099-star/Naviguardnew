// Naviguard.Domain/Entities/ProxyInfo.cs
namespace Naviguard.Domain.Entities
{
    public class ProxyInfo
    {
        public long ProxyId { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        // Constructor
        public ProxyInfo()
        {
        }

        public ProxyInfo(string host, int port, string? username = null, string? password = null)
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
        }

        // ✅ Métodos de dominio que faltaban
        public string GetProxyAddress() => $"{Host}:{Port}";

        public bool RequiresAuthentication() =>
            !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Host) && Port > 0 && Port <= 65535;
        }
    }
}