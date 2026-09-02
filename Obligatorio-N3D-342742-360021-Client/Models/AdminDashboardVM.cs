namespace Obligatorio_N3D_342742_360021_Client.Models
{
    /// <summary>
    /// Aggregated, view-ready data for the Admin landing dashboard (Home/AdminDashboard).
    /// All figures are computed client-side in HomeController from existing API endpoints.
    /// Reuses <see cref="MonthBucket"/>, <see cref="AttentionItem"/> and <see cref="AiCount"/>
    /// from <see cref="MemberDashboardVM"/>.
    /// </summary>
    public class AdminDashboardVM
    {
        // ---- KPI tiles ----
        public int MemberCount { get; set; }
        public int CoordinatorCount { get; set; }
        public int EquipmentUnits { get; set; }        // Σ Quantity
        public int EquipmentAvailable { get; set; }    // Σ availableQuantity
        public int OnLoanNow { get; set; }
        public int OverdueLoans { get; set; }
        public int PendingRequests { get; set; }
        public int ErrorsThisWeek { get; set; }

        // ---- Charts ----
        public List<MonthBucket> ActivityByMonth { get; set; } = new();
        public List<UtilizationBar> EquipmentUtilization { get; set; } = new();
        public List<RoleSlice> RoleMix { get; set; } = new();

        // ---- Panels ----
        public List<AttentionItem> NeedsAttention { get; set; } = new();
        public List<QueueItem> RequestQueue { get; set; } = new();
        public List<AiCount> AiBreakdown { get; set; } = new();
        public List<CoordinatorStat> CoordinatorThroughput { get; set; } = new();
        public List<AuditEntry> RecentActivity { get; set; } = new();

        // ---- Flavour ----
        public string ClubSizeNote { get; set; } = "";

        // ---- Load health ----
        public bool HasAnyData { get; set; }
        public bool PartialLoad { get; set; } // at least one source failed to load
    }

    public class UtilizationBar
    {
        public string Type { get; set; } = string.Empty; // "Telescope"
        public int OnLoan { get; set; }
        public int Available { get; set; }
        public int Total => OnLoan + Available;
    }

    public class RoleSlice
    {
        public string Role { get; set; } = string.Empty; // "Member"
        public int Count { get; set; }
        public string ColorVar { get; set; } = "--color-text-muted"; // design-token name
    }

    public class QueueItem
    {
        public int RequestId { get; set; }
        public string Requester { get; set; } = "—";
        public string NightDate { get; set; } = string.Empty; // preformatted "MMM d, yyyy"
        public string ObjectName { get; set; } = "—";
        public string? AiIndicator { get; set; }              // IDEAL | ADECUADO | NO_RECOMENDABLE | null
        public string Url { get; set; } = "#";
    }

    public class CoordinatorStat
    {
        public string Name { get; set; } = "—";
        public int ThisMonth { get; set; }
        public int AllTime { get; set; }
    }

    public class AuditEntry
    {
        public string Icon { get; set; } = "fa-circle-info"; // Font Awesome class, no "fa-solid"
        public string Text { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;    // email
        public string When { get; set; } = string.Empty;     // relative, e.g. "2h ago"
        public bool IsError { get; set; }
        public string Url { get; set; } = "#";
    }
}
