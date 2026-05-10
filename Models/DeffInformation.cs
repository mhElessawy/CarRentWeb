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

        // Domain Fields
        public string? Domain { get; set; }
        public DateTime? DomainRenewalDate { get; set; }
        public string? DomainUsername { get; set; }
        public string? DomainPassword { get; set; }

        // SSL Fields
        public string? SslUrl { get; set; }
        public DateTime? SslRenewalDate { get; set; }
        public string? SslUsername { get; set; }
        public string? SslPassword { get; set; }

        // Message Service Fields
        public string? MessageUrl { get; set; }
        public DateTime? MessageRenewalDate { get; set; }
        public string? MessageUsername { get; set; }
        public string? MessagePassword { get; set; }
    }
}
