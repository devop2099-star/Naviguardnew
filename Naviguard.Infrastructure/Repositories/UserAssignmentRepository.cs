using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using Npgsql;

namespace Naviguard.Infrastructure.Repositories
{
    public class UserAssignmentRepository : IUserAssignmentRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public UserAssignmentRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<Group>> GetGroupsByUserIdAsync(int userId)
        {
            var groups = new List<Group>();

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT pg.group_id, pg.group_name, pg.description, pg.pin
                FROM browser_app.page_groups pg
                INNER JOIN browser_app.user_page_groups upg ON pg.group_id = upg.group_id
                WHERE upg.external_user_id = @userId AND upg.state = 1 AND pg.state = 1
                ORDER BY pg.pin DESC, pg.group_name;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                groups.Add(new Group
                {
                    GroupId = reader.GetInt64(0),
                    GroupName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Pin = reader.GetInt16(3)
                });
            }

            return groups;
        }

        public async Task AssignGroupsToUserAsync(int userId, List<long> groupIds)
        {
            if (!groupIds.Any()) return;

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var sql = @"
                    INSERT INTO browser_app.user_page_groups (external_user_id, group_id, assigned_at, state)
                    VALUES (@userId, @groupId, NOW(), 1)
                    ON CONFLICT (external_user_id, group_id) DO NOTHING;";

                foreach (var groupId in groupIds)
                {
                    using var cmd = new NpgsqlCommand(sql, conn, transaction);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveGroupFromUserAsync(int userId, long groupId)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                UPDATE browser_app.user_page_groups 
                SET state = 0 
                WHERE external_user_id = @userId AND group_id = @groupId;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@groupId", groupId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}