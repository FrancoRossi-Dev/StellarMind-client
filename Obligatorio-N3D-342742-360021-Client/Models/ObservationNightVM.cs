namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class ObservationNightVM
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Date { get; set; } = string.Empty;       // ISO 8601
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
