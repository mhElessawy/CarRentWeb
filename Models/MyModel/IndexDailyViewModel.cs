using CarRentWeb.Models;

namespace CarRentWeb.Models.MyModel
{
    public class IndexDailyViewModel
    {
        public List<Contract> DueToday { get; set; } = new();
        public List<Contract> PaidToday { get; set; } = new();
        public List<Contract> Others { get; set; } = new();
        public List<Contract> DiscountAlerts { get; set; } = new();
    }
}