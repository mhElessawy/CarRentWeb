using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentWeb.Models
{
    public class DeffInformation
    {
        [Key]
        public int Id { get; set; }
        public double? DebitPayLateDay { get; set; }
        [ForeignKey("DeffPayLate")]  // This points to the navigation property
        public int? DebitPayLatId { get; set; }  // Renamed for clarity
        public virtual Deff? DeffPayLate { get; set; }  // Renamed for consistency

        // VPS Fields
        public DateTime? VpsRenewalDate { get; set; }
        public string? SiteUrl { get; set; }
        public string? SiteUsername { get; set; }
        public string? SitePassword { get; set; }

        // Domain & SSL Fields
        public string? Domain { get; set; }
        public DateTime? SslExpiry { get; set; }
        public string? Message { get; set; }
    }
}
