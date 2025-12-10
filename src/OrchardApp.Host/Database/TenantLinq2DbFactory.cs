using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using Orchard.ModuleBase;

namespace OrchardApp.Host.Database
{
    public class TenantLinq2DbFactory : ITenantScopedFactory<DataConnection>
    {
        private readonly string _providerName;

        public TenantLinq2DbFactory(IConfiguration config)
        {
            // Read provider from configuration, defaults to SQL Server
            _providerName = config.GetValue<string>("LinqToDb:Provider") ?? "SqlServer";
        }

        public DataConnection Create(ITenantContext tenant)
        {
            if (tenant.ConnectionString == null)
                throw new InvalidOperationException("Tenant has no connection string.");

            // Pick data provider
            var provider = _providerName switch
            {
                "SqlServer" => SqlServerTools.GetDataProvider(SqlServerVersion.v2016),
                "PostgreSQL" => LinqToDB.DataProvider.PostgreSQL.PostgreSQLTools.GetDataProvider(),
                "MySql" => LinqToDB.DataProvider.MySql.MySqlTools.GetDataProvider(),
                "SQLite" => LinqToDB.DataProvider.SQLite.SQLiteTools.GetDataProvider(),
                _ => throw new NotSupportedException($"Unsupported provider: {_providerName}")
            };

            // Create the DataConnection using provider + connection string
            return new DataConnection(provider, tenant.ConnectionString);
        }
    }
}
