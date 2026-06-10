namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class PendingLoanRequestVM
    {
        public int RequestId { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected", "Canceled"
        public EquipmentSetVM? EquipmentSet { get; set; }
        public ObservationNightVM? ObservationNight { get; set; }
        public UserVM? RequestingUser { get; set; }
        public string? AiIndicator { get; set; } // "IDEAL", "ADECUADO", "NO_RECOMENDABLE"
        public string? AiDetail { get; set; }    // null when IDEAL, short explanation otherwise
    }

    public class EquipmentSetVM
    {
        public EquipmentItemVM? Telescope { get; set; }
        public EquipmentItemVM? Mount { get; set; }
        public EquipmentItemVM? Eyepiece { get; set; }
        public EquipmentItemVM? Camera { get; set; }
    }

    public class EquipmentItemVM
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
