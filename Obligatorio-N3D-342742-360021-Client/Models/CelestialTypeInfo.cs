namespace Obligatorio_N3D_342742_360021_Client.Models
{
    /// <summary>
    /// Shared label / grouping / colour mapping for CelestialObjectType ints (1–19).
    /// Used by the celestial ranking report and the member dashboard so both stay in sync.
    /// </summary>
    public static class CelestialTypeInfo
    {
        public static string Label(int t) => t switch
        {
            1  => "Star",
            2  => "Neutron Star",
            3  => "White Dwarf",
            4  => "Black Hole",
            5  => "Open Cluster",
            6  => "Globular Cluster",
            7  => "Emission Nebula",
            8  => "Reflection Nebula",
            9  => "Planetary Nebula",
            10 => "Supernova Remnant",
            11 => "Galaxy",
            12 => "Galaxy Cluster",
            13 => "Planet",
            14 => "Dwarf Planet",
            15 => "Moon",
            16 => "Asteroid",
            17 => "Comet",
            18 => "Quasar",
            19 => "Pulsar",
            _  => "Unknown"
        };

        public static string Group(int t) => t switch
        {
            1 or 2 or 3 or 4 or 19 => "Star",
            5 or 6                  => "Cluster",
            7 or 8 or 9 or 10       => "Nebula",
            11 or 12                => "Galaxy",
            13 or 14 or 15          => "Planet",
            16 or 17                => "Small Body",
            18                      => "Quasar",
            _                       => "Other"
        };

        /// <summary>Design-token custom-property name for a type group (resolve with var(...)).</summary>
        public static string GroupColorVar(string group) => group switch
        {
            "Star"       => "--color-warning",
            "Cluster"    => "--color-accent",
            "Nebula"     => "--color-info",
            "Galaxy"     => "--color-success",
            "Planet"     => "--color-text-secondary",
            "Small Body" => "--color-text-muted",
            "Quasar"     => "--color-accent-hover",
            _            => "--color-text-muted"
        };

        public static string ColorVarForType(int t) => GroupColorVar(Group(t));
    }
}
