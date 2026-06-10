namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class CreateLoanTicketDto
    {
        public int LoanRequestId { get; set; }
        public int CoordinatorId { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
    }
}
