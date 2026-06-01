namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class CreateLoanDto
    {
        public int MemberId { get; set; }
        public int EquipmentId { get; set; }
        public string RequestDate { get; set; } = string.Empty;   // ISO 8601
    }
}
