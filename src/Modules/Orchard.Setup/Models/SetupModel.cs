using System.ComponentModel.DataAnnotations;

namespace Orchard.Setup.Models
{
    public class SetupModel
    {
        [Required]
        [Display(Name = "Site name")]
        public string SiteName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Admin user")]
        public string AdminUser { get; set; } = "admin";

        [Required, DataType(DataType.Password)]
        [Display(Name = "Admin password")]
        public string AdminPassword { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Admin email")]
        public string AdminEmail { get; set; } = string.Empty;

        // Optionally add DB connection string input if you want setup to accept it
        [Display(Name = "Database connection string (optional)")]
        public string? DbConnectionString { get; set; }
    }
}
