using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using Npgsql;

namespace Naviguard.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public AuthRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> UserHasRolesAsync(long userId)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT 1 
                FROM browser_app.roles_user 
                WHERE external_user_id = @userId AND state = 1 
                LIMIT 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = await cmd.ExecuteScalarAsync();

            return result != null;
        }
    }
}