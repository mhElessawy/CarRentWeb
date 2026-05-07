using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentWeb.Models
{
    public class DeffInformation
    {
        [Key]
        public int Id { get; set; }
        public double? DebitPayLateDay { get; set; }
        [ForeignKey("DeffPayLate")]
        public int? DebitPayLatId { get; set; }
        public virtual Deff? DeffPayLate { get; set; }

        public DateTime? VpsRenewalDate { get; set; }
        public DateTime? DomainRenewalDate { get; set; }
        public DateTime? SslRenewalDate { get; set; }
        public DateTime? MessageRenewalDate { get; set; }
    }
}
