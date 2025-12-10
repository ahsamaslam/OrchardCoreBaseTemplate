// src/Modules/Orchard.Identity/Models/UserRole.cs
using LinqToDB.Mapping;
using System;

namespace Orchard.Identity.Models
{
    [Table(Name = "UserRoles")]
    public class UserRole
    {
        [Column(Name = "UserId"), PrimaryKey]
        public Guid UserId { get; set; }

        [Column(Name = "RoleId"), PrimaryKey]
        public Guid RoleId { get; set; }
    }
}
