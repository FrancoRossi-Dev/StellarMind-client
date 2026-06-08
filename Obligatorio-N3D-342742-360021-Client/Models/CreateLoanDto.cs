namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class CreateLoanRequestDto
    {
        public int ObservationNightId { get; set; }
        public int TelescopeId { get; set; }
        public int MountId { get; set; }
        public int? EyepieceId { get; set; }
        public int? CameraId { get; set; }
    }
}
