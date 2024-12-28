namespace WEBBERBERODEV.Models
{
    public class EmployeeDailyEarningsViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalAppointments { get; set; }
    }
}
