namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class LoanTicketVM
    {
        public int Id { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public PendingLoanRequestVM? LoanRequest { get; set; }
        public int CoordinatorId { get; set; }
        public string CoordinatorName { get; set; } = string.Empty;
        public string CoordinatorEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Loaned", "Returned", "Canceled"
        public bool IsOverdue { get; set; }
    }
}
