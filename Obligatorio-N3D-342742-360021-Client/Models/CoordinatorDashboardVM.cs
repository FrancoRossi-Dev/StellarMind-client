namespace Obligatorio_N3D_342742_360021_Client.Models
{
    /// <summary>
    /// Aggregated, view-ready data for the Coordinator landing dashboard
    /// (Home/CoordinatorDashboard). All figures are computed client-side in
    /// HomeController from existing API endpoints. Reuses <see cref="MonthBucket"/>,
    /// <see cref="AttentionItem"/>, <see cref="AiCount"/> from
    /// <see cref="MemberDashboardVM"/> and <see cref="QueueItem"/> from
    /// <see cref="AdminDashboardVM"/>.
    /// </summary>
    public class CoordinatorDashboardVM
    {
        // ---- KPI tiles ----
        public int PendingRequests { get; set; }   // shared queue depth
        public int FlaggedByAi { get; set; }       // pending requests the AI rated NO_RECOMENDABLE
        public int OnLoanNow { get; set; }          // this coordinator's tickets currently "Loaned"
        public int DueThisWeek { get; set; }        // their loaned tickets due back within 7 days
        public int OverdueLoans { get; set; }       // their loaned tickets past due
        public int IssuedAllTime { get; set; }      // every ticket they've ever issued

        // ---- Charts ----
        public List<MonthBucket> ApprovalsByMonth { get; set; } = new();
        public List<AiCount> QueueRisk { get; set; } = new();

        // ---- Panels ----
        public List<AttentionItem> NeedsAttention { get; set; } = new();
        public List<QueueItem> RequestQueue { get; set; } = new();
        public List<ReturnDue> ReturnsDue { get; set; } = new();

        // ---- Flavour ----
        public string QueueNote { get; set; } = "";

        // ---- Signals for empty-state wording ----
        public bool HasQueue { get; set; }
        public bool HasTickets { get; set; }

        // ---- Load health ----
        public bool HasAnyData { get; set; }
        public bool PartialLoad { get; set; } // at least one source failed to load
    }

    public class ReturnDue
    {
        public string Member { get; set; } = "—";
        public string ObjectName { get; set; } = "—";
        public string DueDate { get; set; } = string.Empty; // preformatted "MMM d, yyyy"
        public bool IsOverdue { get; set; }
        public string Url { get; set; } = "#";
    }
}
