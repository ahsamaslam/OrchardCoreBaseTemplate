// src/Modules/Orchard.Identity/Migrations/_0001_CreateIdentityTables.cs
using FluentMigrator;

namespace Orchard.Identity.Migrations
{
    [Migration(202512100001)]
    public class _0001_CreateIdentityTables : Migration
    {
        public override void Up()
        {
            // Users table
            Create.Table("Users")
                .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
                .WithColumn("UserName").AsString(256).NotNullable()
                .WithColumn("NormalizedUserName").AsString(256).NotNullable()
                .WithColumn("Email").AsString(256).Nullable()
                .WithColumn("NormalizedEmail").AsString(256).Nullable()
                .WithColumn("EmailConfirmed").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("PasswordHash").AsString(int.MaxValue).Nullable()
                .WithColumn("SecurityStamp").AsString(256).Nullable()
                .WithColumn("ConcurrencyStamp").AsString(256).Nullable()
                .WithColumn("PhoneNumber").AsString(50).Nullable()
                .WithColumn("PhoneNumberConfirmed").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("TwoFactorEnabled").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("LockoutEnd").AsDateTime().Nullable()
                .WithColumn("LockoutEnabled").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("AccessFailedCount").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("CreatedUtc").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            // Roles table
            Create.Table("Roles")
                .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
                .WithColumn("Name").AsString(256).NotNullable()
                .WithColumn("NormalizedName").AsString(256).NotNullable()
                .WithColumn("ConcurrencyStamp").AsString(256).Nullable();

            // UserRoles join table
            Create.Table("UserRoles")
                .WithColumn("UserId").AsGuid().NotNullable()
                .WithColumn("RoleId").AsGuid().NotNullable();

            Create.Index("IX_Users_NormalizedUserName").OnTable("Users").OnColumn("NormalizedUserName").Ascending().WithOptions().Unique();
            Create.Index("IX_Roles_NormalizedName").OnTable("Roles").OnColumn("NormalizedName").Ascending().WithOptions().Unique();
        }

        public override void Down()
        {
            Delete.Table("UserRoles");
            Delete.Table("Roles");
            Delete.Table("Users");
        }
    }
}
