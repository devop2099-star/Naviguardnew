using Naviguard.Domain.Entities;

namespace Naviguard.Domain.Interfaces
{
    public interface IProxyRepository
    {
        Task<ProxyInfo?> GetProxyAsync();
    }
}