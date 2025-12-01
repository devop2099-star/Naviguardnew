using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Npgsql;

namespace Naviguard.Infrastructure.Data
{
    public class ConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public ConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public NpgsqlConnection CreateNaviguardConnection()
        {
            var connectionString = _configuration.GetConnectionString("Naviguard")
                ?? throw new InvalidOperationException("La cadena de conexión 'Naviguard' no se encontró.");

            return new NpgsqlConnection(connectionString);
        }

        public MySqlConnection CreateNexusEcosystemConnection()
        {
            var connectionString = _configuration.GetConnectionString("NexusEcosystem")
                ?? throw new InvalidOperationException("La cadena de conexión 'NexusEcosystem' no se encontró.");

            return new MySqlConnection(connectionString);
        }
    }
}