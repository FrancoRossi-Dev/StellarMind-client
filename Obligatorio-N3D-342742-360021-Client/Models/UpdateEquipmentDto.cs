namespace Obligatorio_N3D_342742_360021_Client.Models
{
    // API update endpoint uses field "equipmentType" (not "type")
    public class UpdateEquipmentDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }

        // Telescope
        public string Aperture { get; set; } = string.Empty;
        public string FocalRatio { get; set; } = string.Empty;
        public string FocalLenght { get; set; } = string.Empty;   // API typo — keep exact spelling
        public string Weight { get; set; } = string.Empty;

        // Camera (not [Required] on the API — keep nullable)
        public string? Resolution { get; set; }
        public string? SensorType { get; set; }
        public string? PixelSize { get; set; }

        // Eyepiece
        public string Diameter { get; set; } = string.Empty;
        public string FieldOfView { get; set; } = string.Empty;

        // Mount
        public string MountType { get; set; } = string.Empty;
        public string WeightSupport { get; set; } = string.Empty;
        public bool IsGoTo { get; set; }
    }
}
