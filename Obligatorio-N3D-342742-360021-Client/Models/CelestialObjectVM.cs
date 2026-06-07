namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class CelestialObjectVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public double AparentMagnitude { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}
