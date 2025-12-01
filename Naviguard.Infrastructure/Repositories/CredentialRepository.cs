using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using Npgsql;
using System.Diagnostics;

namespace Naviguard.Infrastructure.Repositories
{
    public class CredentialRepository : ICredentialRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public CredentialRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UserPageCredential?> GetCredentialAsync(long userId, long pageId)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT pagreqlg_id, external_user_id, page_id, username, password 
                FROM browser_app.pages_requires_login 
                WHERE external_user_id = @userId AND page_id = @pageId AND state = 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@pageId", pageId);

            Debug.WriteLine("--- Ejecutando Consulta SQL ---");
            Debug.WriteLine(cmd.CommandText);
            foreach (NpgsqlParameter p in cmd.Parameters)
            {
                Debug.WriteLine($"--> {p.ParameterName}: {p.Value}");
            }

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserPageCredential
                {
                    PageReqLgId = reader.GetInt64(0),
                    ExternalUserId = reader.GetInt64(1),
                    PageId = reader.GetInt64(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Password = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                };
            }

            return null;
        }

        public async Task UpdateOrInsertCredentialAsync(long userId, long pageId, string username, string password)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO browser_app.pages_requires_login (external_user_id, page_id, username, password, state)
                VALUES (@userId, @pageId, @username, @password, 1)
                ON CONFLICT (external_user_id, page_id) DO UPDATE SET
                    username = EXCLUDED.username,
                    password = EXCLUDED.password;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@pageId", pageId);
            cmd.Parameters.AddWithValue("@username", string.IsNullOrEmpty(username) ? DBNull.Value : username);
            cmd.Parameters.AddWithValue("@password", string.IsNullOrEmpty(password) ? DBNull.Value : password);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}