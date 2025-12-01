using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using Npgsql;
using System.Diagnostics;

namespace Naviguard.Infrastructure.Repositories
{
    public class ProxyRepository : IProxyRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public ProxyRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<ProxyInfo?> GetProxyAsync()
        {
            try
            {
                using var conn = _connectionFactory.CreateNaviguardConnection();
                await conn.OpenAsync();

                var sql = @"
                    SELECT proxy_id, host, port, username, password 
                    FROM browser_app.proxies 
                    WHERE proxy_id IS NOT NULL 
                    LIMIT 1;";

                using var cmd = new NpgsqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new ProxyInfo
                    {
                        ProxyId = reader.GetInt64(0),
                        Host = reader.GetString(1),
                        Port = reader.GetInt32(2),
                        Username = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Password = reader.IsDBNull(4) ? null : reader.GetString(4)
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener proxy: {ex.Message}");
            }

            return null;
        }
    }
}