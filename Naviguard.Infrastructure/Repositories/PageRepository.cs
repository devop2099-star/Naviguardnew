using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using Npgsql;
using System.Diagnostics;

namespace Naviguard.Infrastructure.Repositories
{
    public class PageRepository : IPageRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public PageRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<Pagina>> GetAllPagesAsync()
        {
            var pages = new List<Pagina>();

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT page_id, page_name, description, url, 
                       requires_proxy, requires_login, requires_custom_login, 
                       requires_redirects, state, created_at 
                FROM browser_app.pages 
                WHERE state = 1 
                ORDER BY page_name;";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pages.Add(new Pagina
                {
                    PageId = reader.GetInt64(0),
                    PageName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Url = reader.GetString(3),
                    RequiresProxy = reader.GetBoolean(4),
                    RequiresLogin = reader.GetBoolean(5),
                    RequiresCustomLogin = reader.GetBoolean(6),
                    RequiresRedirects = reader.GetBoolean(7),
                    State = reader.GetInt16(8),
                    CreatedAt = reader.GetDateTime(9)
                });
            }

            return pages;
        }

        public async Task<Pagina?> GetPageByIdAsync(long pageId)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT page_id, page_name, description, url, 
                       requires_proxy, requires_login, requires_custom_login, 
                       requires_redirects, state, created_at 
                FROM browser_app.pages 
                WHERE page_id = @pageId AND state = 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pageId", pageId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Pagina
                {
                    PageId = reader.GetInt64(0),
                    PageName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Url = reader.GetString(3),
                    RequiresProxy = reader.GetBoolean(4),
                    RequiresLogin = reader.GetBoolean(5),
                    RequiresCustomLogin = reader.GetBoolean(6),
                    RequiresRedirects = reader.GetBoolean(7),
                    State = reader.GetInt16(8),
                    CreatedAt = reader.GetDateTime(9)
                };
            }

            return null;
        }

        public async Task<List<Pagina>> GetPagesRequiringCustomLoginAsync()
        {
            Debug.WriteLine("[PageRepository] Buscando páginas con login personalizado...");

            var pages = new List<Pagina>();

            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT page_id, page_name, description, url, 
                       requires_proxy, requires_login, requires_custom_login, 
                       requires_redirects, state, created_at 
                FROM browser_app.pages 
                WHERE requires_custom_login = true AND state = 1 
                ORDER BY page_name;";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pages.Add(new Pagina
                {
                    PageId = reader.GetInt64(0),
                    PageName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Url = reader.GetString(3),
                    RequiresProxy = reader.GetBoolean(4),
                    RequiresLogin = reader.GetBoolean(5),
                    RequiresCustomLogin = reader.GetBoolean(6),
                    RequiresRedirects = reader.GetBoolean(7),
                    State = reader.GetInt16(8),
                    CreatedAt = reader.GetDateTime(9)
                });
            }

            Debug.WriteLine($"[PageRepository] Se encontraron {pages.Count} páginas");
            return pages;
        }

        public async Task<long> AddPageAsync(Pagina page)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO browser_app.pages 
                (page_name, description, url, requires_proxy, requires_login, 
                 requires_custom_login, requires_redirects, state, created_at) 
                VALUES 
                (@pageName, @description, @url, @requiresProxy, @requiresLogin, 
                 @requiresCustomLogin, @requiresRedirects, @state, @createdAt)
                RETURNING page_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pageName", page.PageName);
            cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(page.Description) ? DBNull.Value : page.Description);
            cmd.Parameters.AddWithValue("@url", page.Url);
            cmd.Parameters.AddWithValue("@requiresProxy", page.RequiresProxy);
            cmd.Parameters.AddWithValue("@requiresLogin", page.RequiresLogin);
            cmd.Parameters.AddWithValue("@requiresCustomLogin", page.RequiresCustomLogin);
            cmd.Parameters.AddWithValue("@requiresRedirects", page.RequiresRedirects);
            cmd.Parameters.AddWithValue("@state", page.State);
            cmd.Parameters.AddWithValue("@createdAt", page.CreatedAt);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public async Task UpdatePageAsync(Pagina page)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                UPDATE browser_app.pages
                SET page_name = @pageName,
                    description = @description,
                    url = @url,
                    requires_proxy = @requiresProxy,
                    requires_login = @requiresLogin,
                    requires_custom_login = @requiresCustomLogin,
                    requires_redirects = @requiresRedirects
                WHERE page_id = @pageId;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pageName", page.PageName);
            cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(page.Description) ? DBNull.Value : page.Description);
            cmd.Parameters.AddWithValue("@url", page.Url);
            cmd.Parameters.AddWithValue("@requiresProxy", page.RequiresProxy);
            cmd.Parameters.AddWithValue("@requiresLogin", page.RequiresLogin);
            cmd.Parameters.AddWithValue("@requiresCustomLogin", page.RequiresCustomLogin);
            cmd.Parameters.AddWithValue("@requiresRedirects", page.RequiresRedirects);
            cmd.Parameters.AddWithValue("@pageId", page.PageId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SoftDeletePageAsync(long pageId)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = "UPDATE browser_app.pages SET state = 0 WHERE page_id = @pageId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pageId", pageId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddCredentialAsync(long pageId, string username, string password)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO browser_app.page_credentials
                (page_id, username, password, created_at, state)
                VALUES
                (@pageId, @username, @password, @createdAt, @state);";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pageId", pageId);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", string.IsNullOrEmpty(password) ? DBNull.Value : password);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@state", 1);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateOrInsertCredentialAsync(long pageId, string username, string password)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO browser_app.page_credentials (page_id, username, password, created_at, state)
                VALUES (@pageId, @username, @password, @createdAt, 1)
                ON CONFLICT (page_id) DO UPDATE SET
                    username = EXCLUDED.username,
                    password = EXCLUDED.password;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pageId", pageId);
            cmd.Parameters.AddWithValue("@username", string.IsNullOrEmpty(username) ? DBNull.Value : username);
            cmd.Parameters.AddWithValue("@password", string.IsNullOrEmpty(password) ? DBNull.Value : password);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<PageCredential?> GetCredentialForPageAsync(long pageId)
        {
            Debug.WriteLine($"[PageRepository] Buscando credenciales para PageID: {pageId}");

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
                Debug.WriteLine($"[PageRepository] ✅ Credencial encontrada. Usuario: '{reader.GetString(2)}'");
                return new PageCredential
                {
                    PageCredentialId = reader.GetInt64(0),
                    PageId = reader.GetInt64(1),
                    Username = reader.GetString(2),
                    Password = reader.GetString(3),
                    State = reader.GetInt16(4)
                };
            }

            Debug.WriteLine($"[PageRepository] ❌ No se encontró credencial");
            return null;
        }

        public async Task AddPagesToGroupAsync(long groupId, List<long> pageIds)
        {
            using var conn = _connectionFactory.CreateNaviguardConnection();
            await conn.OpenAsync();

            foreach (var pageId in pageIds)
            {
                var sql = @"
                    INSERT INTO browser_app.group_pages (group_id, page_id, state)
                    VALUES (@groupId, @pageId, @state);";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@groupId", groupId);
                cmd.Parameters.AddWithValue("@pageId", pageId);
                cmd.Parameters.AddWithValue("@state", 1);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}