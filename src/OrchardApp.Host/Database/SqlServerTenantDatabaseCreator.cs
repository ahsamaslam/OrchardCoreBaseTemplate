using Microsoft.Data.SqlClient;
using Orchard.ModuleBase.Tenant;

namespace OrchardApp.Host.Database
{
    public class SqlServerTenantDatabaseCreator : ITenantDatabaseCreator
    {
        private readonly ILogger<SqlServerTenantDatabaseCreator> _logger;
        public SqlServerTenantDatabaseCreator(ILogger<SqlServerTenantDatabaseCreator> logger) => _logger = logger;

        public async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct = default)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
                throw new InvalidOperationException("Connection string must include Initial Catalog / database name.");

            var dbName = builder.InitialCatalog;
            var masterCs = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ConnectionString;

            // Try open tenant DB - if success, return
            try
            {
                await using var test = new SqlConnection(connectionString);
                await test.OpenAsync(ct);
                _logger.LogInformation("Tenant DB {Db} exists and is open.", dbName);
                return;
            }
            catch (SqlException ex) when (ex.Number == 4060 || ex.Message.Contains("Cannot open database"))
            {
                _logger.LogInformation("Tenant DB {Db} not open (will attempt create). Msg: {Msg}", dbName, ex.Message);
            }

            // Create DB on master
            var sql = $"IF DB_ID(N'{dbName}') IS NULL CREATE DATABASE [{dbName}];";
            await using var cn = new SqlConnection(masterCs);
            await cn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, cn);
            await cmd.ExecuteNonQueryAsync(ct);
            _logger.LogInformation("Created database {Db}.", dbName);
        }
    }
}
