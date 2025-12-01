using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using System.Diagnostics;

namespace Naviguard.Infrastructure.ExternalServices
{
    public class ProxyManager
    {
        private readonly IProxyRepository _proxyRepository;

        public ProxyManager(IProxyRepository proxyRepository)
        {
            _proxyRepository = proxyRepository;
        }

        public async Task<ProxyInfo?> GetProxyAsync()
        {
            try
            {
                return await _proxyRepository.GetProxyAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener proxy: {ex.Message}");
                return null;
            }
        }
    }
}