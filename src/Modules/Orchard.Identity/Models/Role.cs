// src/Modules/Orchard.Identity/Models/Role.cs
using LinqToDB.Mapping;
using System;

namespace Orchard.Identity.Models
{
    [Table(Name = "Roles")]
    public class Role
    {
        [Column(Name = "Id"), PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(Name = "Name"), NotNull]
        public string Name { get; set; } = null!;

        [Column(Name = "NormalizedName"), NotNull]
        public string NormalizedName { get; set; } = null!;

        [Column(Name = "ConcurrencyStamp")]
        public string? ConcurrencyStamp { get; set; }
    }
}
