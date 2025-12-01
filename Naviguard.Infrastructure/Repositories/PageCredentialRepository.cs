using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using Npgsql;
using System.Diagnostics;

namespace Naviguard.Infrastructure.Repositories
{
    public class PageCredentialRepository : IPageCredentialRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public PageCredentialRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PageCredential?> GetCredentialByPageIdAsync(long pageId)
        {
            Debug.WriteLine($"[PageCredentialRepository] Buscando credencial para PageId: {pageId}");

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT page_credential_id, page_id, username, password, state 
                FROM browser_app.page_credentials 
                WHERE page_id = @pageId AND state = 1 
                LIMIT 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pageId", pageId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Debug.WriteLine($"[PageCredentialRepository] ✅ Credencial encontrada. Usuario: '{reader.GetString(2)}'");
                return new PageCredential
                {
                    PageCredentialId = reader.GetInt64(0),
                    PageId = reader.GetInt64(1),
                    Username = reader.GetString(2),
                    Password = reader.GetString(3),
                    State = reader.GetInt16(4)
                };
            }

            Debug.WriteLine($"[PageCredentialRepository] ❌ No se encontró credencial");
            return null;
        }
    }
}