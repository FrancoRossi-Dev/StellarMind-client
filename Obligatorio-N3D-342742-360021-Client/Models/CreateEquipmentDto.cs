namespace Obligatorio_N3D_342742_360021_Client.Models
{
    // API create endpoint uses field "type" (not "equipmentType")
    public class CreateEquipmentDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }

        // Telescope
        public string? Aperture { get; set; }
        public string? FocalRatio { get; set; }
        public string? FocalLenght { get; set; }   // API typo — keep exact spelling
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
        public bool IsGoTo { get; set; }
    }
}
