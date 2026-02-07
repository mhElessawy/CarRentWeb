using Microsoft.EntityFrameworkCore;

namespace CarRentWeb.Models
{
    public class EmployeeSalary
    {
        public int Id { get; set; }
        public int EmpId { get; set; }
        public int EmpSalaryYear  { get; set; }
        public int EmpSalaryMonth { get;set; }
        public DateOnly  EntryDate { get; set; }

        [Precision(18, 2)]
        public decimal EmpSalary { get; set; }
        [Precision(18, 2)]
        public decimal  EmpSpecial { get; set; }
        [Precision(18, 2)]
        public decimal AddBasicSalary { get; set; }
        [Precision(18, 2)]
        public decimal AddFood { get; set; }
        [Precision(18, 2)]
        public decimal AddSpecial { get; set; }
        [Precision(18, 2)]  
        public decimal AddTravel { get; set; }
        [Precision(18, 2)]      
        public decimal AddRoom { get; set; }

        [Precision(18, 2)]
        public decimal DeductPenality { get; set; }
        [Precision(18, 2)]
        public decimal DeductAdvance { get; set; }
        [Precision(18, 2)]
        public decimal DeductRoom { get; set; }
        [Precision(18, 2)]
        public decimal TotalPayed { get; set; }
        [Precision(18, 2)]
        public decimal TotalDeduuct { get; set; }
        [Precision(18, 2)]
        public decimal Net { get; set; }
        public int Approve { get; set; }
        public int SalaryRecieved { get; set; }
        public int UserId { get; set; }

        public virtual PasswordDatum? User { get; set; }

        public virtual EmployeeInfo? Emp { get; set; }
    }
}
