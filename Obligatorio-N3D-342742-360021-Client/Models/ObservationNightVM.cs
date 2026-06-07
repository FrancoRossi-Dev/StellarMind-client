namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class ObservationNightVM
    {
        public int Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public CelestialObjectVM? CelestialObject { get; set; }
        public string Details { get; set; } = string.Empty;
        public int RequestingUserId { get; set; }
        public bool IsRequested { get; set; }
    }
}
