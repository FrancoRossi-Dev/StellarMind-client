namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class EquipmentVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double? FocalLenght { get; set; }   // API typo — keep exact spelling
        public string? MountType { get; set; }
        public string? SensorType { get; set; }
    }
}
