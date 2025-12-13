using MySqlConnector;
using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using System.Text;

namespace Naviguard.Infrastructure.Repositories
{
    public class BusinessStructureRepository : IBusinessStructureRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public BusinessStructureRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<BusinessDepartment>> GetDepartmentsAsync()
        {
            var departments = new List<BusinessDepartment>();

            using var conn = _connectionFactory.CreateNexusEcosystemConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT id_bnsdpt, name_department 
                FROM business_department 
                WHERE state = 1 
                ORDER BY name_department;";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                departments.Add(new BusinessDepartment
                {
                    DepartmentId = reader.GetInt32(0),
                    DepartmentName = reader.GetString(1)
                });
            }

            return departments;
        }

        public async Task<List<BusinessArea>> GetAreasByDepartmentAsync(int departmentId)
        {
            var areas = new List<BusinessArea>();

            using var conn = _connectionFactory.CreateNexusEcosystemConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT id_bnsarea, name_area, id_bnsdpt 
                FROM business_area 
                WHERE state = 1 AND id_bnsdpt = @departmentId 
                ORDER BY name_area;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@departmentId", departmentId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                areas.Add(new BusinessArea
                {
                    AreaId = reader.GetInt32(0),
                    AreaName = reader.GetString(1),
                    DepartmentId = reader.GetInt32(2)
                });
            }

            return areas;
        }

        public async Task<List<BusinessSubarea>> GetSubareasByAreaAsync(int areaId)
        {
            var subareas = new List<BusinessSubarea>();

            using var conn = _connectionFactory.CreateNexusEcosystemConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT id_bnsbar, name_subarea, id_bnsarea 
                FROM business_subarea 
                WHERE state = 1 AND id_bnsarea = @areaId 
                ORDER BY name_subarea;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@areaId", areaId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                subareas.Add(new BusinessSubarea
                {
                    SubareaId = reader.GetInt32(0),
                    SubareaName = reader.GetString(1),
                    AreaId = reader.GetInt32(2)
                });
            }

            return subareas;
        }

        public async Task<List<FilteredUser>> FilterUsersAsync(
            string? name,
            int? departmentId,
            int? areaId,
            int? subareaId)
        {
            var users = new List<FilteredUser>();

            using var conn = _connectionFactory.CreateNexusEcosystemConnection();
            await conn.OpenAsync();

            var sqlBuilder = new StringBuilder(@"
                SELECT md.id_user, 
                       CONCAT_WS(' ', md.name, COALESCE(NULLIF(md.paternal_surname, ''), md.maternal_surname)) AS full_name
                FROM mother_data md
                INNER JOIN relational_database rd ON md.id_user = rd.id_user
                INNER JOIN business_information bi ON rd.id_bsninfo = bi.id_bsninfo
            ");

            var conditions = new List<string>();
            var parameters = new List<MySqlParameter>();

            conditions.Add("md.state = 1");

            if (!string.IsNullOrWhiteSpace(name))
            {
                conditions.Add("CONCAT_WS(' ', md.name, md.paternal_surname, md.maternal_surname) LIKE @name");
                parameters.Add(new MySqlParameter("@name", $"%{name}%"));
            }

            if (departmentId.HasValue)
            {
                conditions.Add("bi.id_bnsdpt = @departmentId");
                parameters.Add(new MySqlParameter("@departmentId", departmentId.Value));
            }

            if (areaId.HasValue)
            {
                conditions.Add("bi.id_bnsarea = @areaId");
                parameters.Add(new MySqlParameter("@areaId", areaId.Value));
            }

            if (subareaId.HasValue)
            {
                conditions.Add("bi.id_bnsbar = @subareaId");
                parameters.Add(new MySqlParameter("@subareaId", subareaId.Value));
            }

            if (conditions.Any())
            {
                sqlBuilder.Append(" WHERE ").Append(string.Join(" AND ", conditions));
            }

            sqlBuilder.Append(" ORDER BY full_name;");

            using var cmd = new MySqlCommand(sqlBuilder.ToString(), conn);

            if (parameters.Any())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
            }

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new FilteredUser
                {
                    UserId = reader.GetInt32(0),
                    FullName = reader.GetString(1)
                });
            }

            return users;
        }

        public async Task<List<FilteredUser>> GetUsersByIdsAsync(List<long> userIds)
        {
            var users = new List<FilteredUser>();
            if (userIds == null || !userIds.Any()) return users;

            using var conn = _connectionFactory.CreateNexusEcosystemConnection();
            await conn.OpenAsync();

            var idsString = string.Join(",", userIds);
            
            // Usamos una consulta directa con IN porque parametría lista en MySQL puede ser complejo con ADO.NET puro
            // y los IDs son numéricos controlados (long), riesgo bajo de inyección.
            var sql = $@"
                SELECT md.id_user, 
                       CONCAT_WS(' ', md.name, COALESCE(NULLIF(md.paternal_surname, ''), md.maternal_surname)) AS full_name
                FROM mother_data md
                WHERE md.id_user IN ({idsString})";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new FilteredUser
                {
                    UserId = reader.GetInt32(0),
                    FullName = reader.GetString(1)
                });
            }

            return users;
        }
    }
}