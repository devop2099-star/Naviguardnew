using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Domain.ValueObjects;
using Naviguard.Infrastructure.Data;
using Npgsql;
using System.Diagnostics;

namespace Naviguard.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public GroupRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            var groups = new List<Group>();

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT group_id, group_name, description, pin 
                FROM browser_app.page_groups 
                WHERE state = 1 
                ORDER BY pin DESC, group_name;";

            using var cmd = new NpgsqlCommand(sql, conn);
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

        public async Task<List<Group>> GetGroupsWithPagesAsync()
        {
            var groups = new Dictionary<long, Group>();

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT pg.group_id, pg.group_name, pg.description, pg.pin,
                       p.page_id, p.page_name, p.url, p.requires_proxy, 
                       p.requires_login, p.requires_custom_login, p.requires_redirects,
                       gp.pin AS group_page_pin
                FROM browser_app.page_groups pg
                LEFT JOIN browser_app.group_pages gp ON pg.group_id = gp.group_id
                LEFT JOIN browser_app.pages p ON gp.page_id = p.page_id
                WHERE pg.state = 1 AND (p.state = 1 OR p.page_id IS NULL)
                ORDER BY pg.pin DESC, pg.group_name, p.page_name;";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                long groupId = reader.GetInt64(0);

                if (!groups.ContainsKey(groupId))
                {
                    groups[groupId] = new Group
                    {
                        GroupId = groupId,
                        GroupName = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Pin = reader.GetInt16(3),
                        Pages = new List<Pagina>(),
                        PinnedPageIds = new HashSet<long>()
                    };
                }

                if (!reader.IsDBNull(4)) // Si hay página asociada
                {
                    var page = new Pagina
                    {
                        PageId = reader.GetInt64(4),
                        PageName = reader.GetString(5),
                        Url = reader.GetString(6),
                        RequiresProxy = reader.GetBoolean(7),
                        RequiresLogin = reader.GetBoolean(8),
                        RequiresCustomLogin = reader.GetBoolean(9),
                        RequiresRedirects = reader.GetBoolean(10)
                    };

                    groups[groupId].Pages.Add(page);

                    var pinStatus = reader.IsDBNull(11) ? (short)0 : reader.GetInt16(11);
                    if (pinStatus == 1)
                    {
                        groups[groupId].PinnedPageIds.Add(page.PageId);
                    }
                }
            }

            return groups.Values.ToList();
        }

        public async Task<Group?> GetGroupByIdAsync(long groupId)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT group_id, group_name, description, pin 
                FROM browser_app.page_groups 
                WHERE group_id = @groupId AND state = 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@groupId", groupId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Group
                {
                    GroupId = reader.GetInt64(0),
                    GroupName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Pin = reader.GetInt16(3)
                };
            }

            return null;
        }

        public async Task<List<Pagina>> GetPagesByGroupIdAsync(long groupId)
        {
            var pages = new List<Pagina>();

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT p.page_id, p.page_name, p.url, 
                       p.requires_proxy, p.requires_login, p.requires_custom_login, 
                       p.requires_redirects, p.state, gp.pin
                FROM browser_app.pages p
                INNER JOIN browser_app.group_pages gp ON p.page_id = gp.page_id
                WHERE gp.group_id = @groupId AND p.state = 1
                ORDER BY gp.pin DESC, p.page_name;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@groupId", groupId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pages.Add(new Pagina
                {
                    PageId = reader.GetInt64(0),
                    PageName = reader.GetString(1),
                    Url = reader.GetString(2),
                    RequiresProxy = reader.GetBoolean(3),
                    RequiresLogin = reader.GetBoolean(4),
                    RequiresCustomLogin = reader.GetBoolean(5),
                    RequiresRedirects = reader.GetBoolean(6),
                    State = reader.GetInt16(7),
                    PinInGroup = reader.IsDBNull(8) ? (short)0 : reader.GetInt16(8)
                });
            }

            return pages;
        }

        public async Task<long> AddGroupAsync(string groupName, string description)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO browser_app.page_groups (group_name, description, created_at, state)
                VALUES (@groupName, @description, @createdAt, @state)
                RETURNING group_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@groupName", groupName);
            cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? DBNull.Value : description);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@state", 1);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public async Task UpdateGroupAsync(Group group, List<PageAssignmentInfo> pages)
        {
            Debug.WriteLine($"[GroupRepository] Actualizando grupo ID: {group.GroupId}");

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // 1. Actualizar datos del grupo
                var updateGroupSql = @"
                    UPDATE browser_app.page_groups
                    SET group_name = @groupName, 
                        description = @description, 
                        pin = @pin
                    WHERE group_id = @groupId;";

                using (var cmd = new NpgsqlCommand(updateGroupSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@groupName", group.GroupName);
                    cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(group.Description) ? DBNull.Value : group.Description);
                    cmd.Parameters.AddWithValue("@pin", group.Pin);
                    cmd.Parameters.AddWithValue("@groupId", group.GroupId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. Eliminar todas las asignaciones de páginas
                var deletePagesSql = "DELETE FROM browser_app.group_pages WHERE group_id = @groupId;";
                using (var cmd = new NpgsqlCommand(deletePagesSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@groupId", group.GroupId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. Re-insertar páginas asignadas
                if (pages.Any())
                {
                    var insertPageSql = @"
                        INSERT INTO browser_app.group_pages (group_id, page_id, state, pin) 
                        VALUES (@groupId, @pageId, 1, @pin);";

                    foreach (var pageInfo in pages)
                    {
                        using var cmd = new NpgsqlCommand(insertPageSql, conn, transaction);
                        cmd.Parameters.AddWithValue("@groupId", group.GroupId);
                        cmd.Parameters.AddWithValue("@pageId", pageInfo.PageId);
                        cmd.Parameters.AddWithValue("@pin", pageInfo.IsPinned ? 1 : 0);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                Debug.WriteLine("[GroupRepository] Transacción completada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Debug.WriteLine($"[GroupRepository] Error: {ex.Message}");
                throw;
            }
        }

        public async Task SoftDeleteGroupAsync(long groupId)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = "UPDATE browser_app.page_groups SET state = 0 WHERE group_id = @groupId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@groupId", groupId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}