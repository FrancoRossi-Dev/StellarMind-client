namespace Obligatorio_N3D_342742_360021_Client.Models
{
    /// <summary>
    /// Aggregated, view-ready data for the Member landing dashboard (Home/MemberDashboard).
    /// All figures are computed client-side in HomeController from existing API endpoints.
    /// </summary>
    public class MemberDashboardVM
    {
        // ---- KPI tiles ----
        public int NightsLogged { get; set; }
        public int NightsInPlanning { get; set; }
        public int DistinctObjects { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public int PendingRequests { get; set; }

        // ---- Charts ----
        public List<MonthBucket> ActivityByMonth { get; set; } = new();
        public List<SkySlice> SkyComposition { get; set; } = new();

        // ---- Panels ----
        public List<UpcomingNight> Upcoming { get; set; } = new();
        public List<AttentionItem> NeedsAttention { get; set; } = new();
        public List<AiCount> AiBreakdown { get; set; } = new();

        // ---- Flavour ----
        public string StargazerLevel { get; set; } = "Cadet";
        public string NextLevelName { get; set; } = "";
        public int NextLevelAt { get; set; }   // total nights needed for the next tier; 0 when maxed

        // ---- Signals for empty-state wording ----
        public bool HasNights { get; set; }
        public bool HasRequests { get; set; }

        // ---- Load health ----
        public bool HasAnyData { get; set; }
        public bool PartialLoad { get; set; } // at least one source failed to load
    }

    public class MonthBucket
    {
        public string Label { get; set; } = string.Empty; // "Sep"
        public int Count { get; set; }                     // observation nights that month
        public int Cumulative { get; set; }                // distinct objects discovered up to the end of that month
    }

    public class SkySlice
    {
        public string Group { get; set; } = string.Empty;    // "Galaxy"
        public int Count { get; set; }
        public string ColorVar { get; set; } = "--color-text-muted"; // design-token name
    }

    public class UpcomingNight
    {
        public int NightId { get; set; }
        public string Date { get; set; } = string.Empty; // preformatted "MMM d, yyyy"
        public string ObjectName { get; set; } = "—";
        public bool EquipmentSecured { get; set; }
    }

    public class AttentionItem
    {
        public string Icon { get; set; } = "fa-circle-info"; // Font Awesome class, no "fa-solid"
        public string Text { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";        // info | warning | danger
        public string Url { get; set; } = "#";
    }

    public class AiCount
    {
        public string Indicator { get; set; } = string.Empty; // IDEAL | ADECUADO | NO_RECOMENDABLE
        public int Count { get; set; }
    }
}
