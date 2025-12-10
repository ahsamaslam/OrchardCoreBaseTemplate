using LinqToDB.Data;
using Orchard.ModuleBase;
using System;

namespace OrchardApp.Host.Database
{
    public abstract class RepositoryBase
    {
        private readonly Func<DataConnection> _connectionFactory;
        private readonly ITenantContext _tenant;

        protected RepositoryBase(Func<DataConnection> connectionFactory, ITenantContext tenant)
        {
            _connectionFactory = connectionFactory;
            _tenant = tenant;
        }

        protected DataConnection CreateConnection() => _connectionFactory();

        protected string TenantId => _tenant.TenantId;
    }
}
