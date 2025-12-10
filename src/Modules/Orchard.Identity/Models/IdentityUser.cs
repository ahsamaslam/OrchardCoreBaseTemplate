// src/Modules/Orchard.Identity/Models/IdentityUser.cs
using LinqToDB.Mapping;
using System;

namespace Orchard.Identity.Models
{
    [Table(Name = "Users")]
    public class IdentityUser
    {
        [Column(Name = "Id"), PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(Name = "UserName"), NotNull]
        public string UserName { get; set; } = null!;

        [Column(Name = "NormalizedUserName"), NotNull]
        public string NormalizedUserName { get; set; } = null!;

        [Column(Name = "Email")]
        public string? Email { get; set; }

        [Column(Name = "NormalizedEmail")]
        public string? NormalizedEmail { get; set; }

        [Column(Name = "EmailConfirmed"), NotNull]
        public bool EmailConfirmed { get; set; }

        [Column(Name = "PasswordHash")]
        public string? PasswordHash { get; set; }

        [Column(Name = "SecurityStamp")]
        public string? SecurityStamp { get; set; }

        [Column(Name = "ConcurrencyStamp")]
        public string? ConcurrencyStamp { get; set; }

        [Column(Name = "PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [Column(Name = "PhoneNumberConfirmed"), NotNull]
        public bool PhoneNumberConfirmed { get; set; }

        [Column(Name = "TwoFactorEnabled"), NotNull]
        public bool TwoFactorEnabled { get; set; }

        [Column(Name = "LockoutEnd")]
        public DateTime? LockoutEnd { get; set; }

        [Column(Name = "LockoutEnabled"), NotNull]
        public bool LockoutEnabled { get; set; }

        [Column(Name = "AccessFailedCount"), NotNull]
        public int AccessFailedCount { get; set; }

        [Column(Name = "CreatedUtc"), NotNull]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
