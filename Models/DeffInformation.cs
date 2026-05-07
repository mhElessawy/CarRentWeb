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

        public DateOnly? VpsRenewalDate { get; set; }
        public DateOnly? DomainRenewalDate { get; set; }
        public DateOnly? SslRenewalDate { get; set; }
        public DateOnly? MessageRenewalDate { get; set; }
    }
}
