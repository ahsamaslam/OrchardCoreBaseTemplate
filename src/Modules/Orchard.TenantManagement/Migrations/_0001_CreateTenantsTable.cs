// src/Modules/Orchard.TenantManagement/Migrations/_0001_CreateTenantsTable.cs
using FluentMigrator;

namespace Orchard.TenantManagement.Migrations
{
    [Migration(202512100002)]
    public class _0001_CreateTenantsTable : Migration
    {
        private const string TableName = "Tenants";

        public override void Up()
        {
            if (!Schema.Table(TableName).Exists())
            {
                Create.Table(TableName)
                    .WithColumn("TenantId").AsString(100).PrimaryKey()
                    .WithColumn("TenantName").AsString(256).NotNullable()
                    .WithColumn("ConnectionString").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Hosts").AsString(1000).Nullable()
                    .WithColumn("SettingsJson").AsString(int.MaxValue).Nullable()
                    .WithColumn("CreatedUtc").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime)
                    .WithColumn("IsActive").AsBoolean().WithDefaultValue(true);
            }
        }

        public override void Down()
        {
            if (Schema.Table(TableName).Exists())
            {
                Delete.Table(TableName);
            }
        }
    }
}
