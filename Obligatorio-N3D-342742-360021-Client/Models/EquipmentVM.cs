namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class EquipmentVM
    {
        // Base
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
        public int availableQuantity { get; set; }
        public bool IsAvailable { get; set; }

        // Telescope
        public string? Aperture { get; set; }
        public string? FocalRatio { get; set; }
        public string? FocalLenght { get; set; }
        public string? Weight { get; set; }

        // Camera
        public string? Resolution { get; set; }
        public string? SensorType { get; set; }
        public string? PixelSize { get; set; }

        // Eyepiece
        public string? Diameter { get; set; }
        public string? FieldOfView { get; set; }

        // Mount
        public string? MountType { get; set; }
        public string? WeightSupport { get; set; }
        public bool IsGoTo { get; set; } = false;
    }
}
