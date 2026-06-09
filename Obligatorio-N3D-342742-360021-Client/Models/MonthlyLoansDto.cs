namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class MonthlyLoansDto
    {
        public List<LoanTicketVM> Loans { get; set; } = new();
        public string? Message { get; set; }
    }
}
