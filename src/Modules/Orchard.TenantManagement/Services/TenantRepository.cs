using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Orchard.ModuleBase;
using Orchard.ModuleBase.Tenant;
using Orchard.TenantManagement.Entities;
using Orchard.TenantManagement.Models;

namespace Orchard.TenantManagement.Services
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ITenantScopedFactory<DataConnection> _dbFactory;
        private readonly ITenantContext _hostTenantContext;

        public TenantRepository(
            ITenantScopedFactory<DataConnection> dbFactory,
            IConfiguration configuration)
        {
            _dbFactory = dbFactory;

            var hostConn = configuration.GetConnectionString("Host")
                ?? throw new InvalidOperationException("ConnectionStrings:Host is required.");

            // Create a Host tenant context for repository usage
            _hostTenantContext = new TenantContext(
                tenantId: "host",
                tenantName: "Host",
                connectionString: hostConn,
                settings: new Dictionary<string, string>());
        }

        private DataConnection CreateDb() => _dbFactory.Create(_hostTenantContext);

        public async Task AddAsync(TenantInfo tenant)
        {
            using var db = CreateDb();

            var exists = await db.GetTable<Tenants>()
                                 .Where(x => x.TenantId == tenant.TenantId)
                                 .AnyAsync();

            if (exists)
            {
                await db.GetTable<Tenants>()
                        .Where(x => x.TenantId == tenant.TenantId)
                        .Set(x => x.TenantName, tenant.TenantName)
                        .Set(x => x.ConnectionString, tenant.ConnectionString)
                        .Set(x => x.Hosts, tenant.Hosts)
                        .Set(x => x.SettingsJson, tenant.SettingsJson)
                        .Set(x => x.IsActive, tenant.IsActive)
                        .UpdateAsync();
            }
            else
            {
                await db.InsertAsync(new Tenants
                {
                    TenantId = tenant.TenantId,
                    TenantName = tenant.TenantName,
                    ConnectionString = tenant.ConnectionString,
                    Hosts = tenant.Hosts,
                    SettingsJson = tenant.SettingsJson,
                    CreatedUtc = DateTime.UtcNow,
                    IsActive = tenant.IsActive
                });
            }
        }

        public async Task<TenantInfo?> FindByIdAsync(string tenantId)
        {
            using var db = CreateDb();

            var rec = await db.GetTable<Tenants>()
                              .Where(x => x.TenantId == tenantId)
                              .FirstOrDefaultAsync();

            return rec?.ToModel();
        }

        public async Task<TenantInfo?> FindByHostAsync(string host)
        {
            using var db = CreateDb();

            var rec = await db.GetTable<Tenants>()
                              .Where(x => x.Hosts!.Contains(host) || x.TenantName == host)
                              .FirstOrDefaultAsync();

            return rec?.ToModel();
        }

        public async Task<IEnumerable<TenantInfo>> ListAsync()
        {
            using var db = CreateDb();

            var list = await db.GetTable<Tenants>()
                               .OrderByDescending(x => x.CreatedUtc)
                               .ToListAsync();

            return list.Select(x => x.ToModel());
        }

        public async Task<bool> RemoveAsync(string tenantId)
        {
            using var db = CreateDb();

            var rows = await db.GetTable<Tenants>()
                               .Where(x => x.TenantId == tenantId)
                               .DeleteAsync();

            return rows > 0;
        }

        // === Linq2DB Table Mapping ===
        
    }
}
