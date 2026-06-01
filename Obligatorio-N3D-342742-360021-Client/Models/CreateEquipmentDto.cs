namespace Obligatorio_N3D_342742_360021_Client.Models
{
    // API create endpoint expects field named "type" (not "equipmentType")
    public class CreateEquipmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double? FocalLenght { get; set; }   // API typo — keep exact spelling
        public string? MountType { get; set; }
        public string? SensorType { get; set; }
    }
}
