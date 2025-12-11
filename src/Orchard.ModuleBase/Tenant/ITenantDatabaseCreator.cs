using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.ModuleBase.Tenant
{
    public interface ITenantDatabaseCreator
    {
        Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct = default);
    }
}
