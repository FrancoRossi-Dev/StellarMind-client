namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class LoanVM
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int EquipmentId { get; set; }
        public int RequestStatus { get; set; }  // 0=Pending, 1=Approved, 2=Rejected, 3=Canceled
        public int LoanStatus { get; set; }     // 0=Loaned, 1=Canceled, 2=Returned
        public string RequestDate { get; set; } = string.Empty;
        public string? ApprovalDate { get; set; }
        public string? ReturnDate { get; set; }
    }
}
