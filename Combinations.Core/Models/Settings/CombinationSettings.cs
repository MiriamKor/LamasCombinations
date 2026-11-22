namespace Combinations.Models.Settings
{
    public class CombinationSettings
    {
        public const string SectionName = "CombinationSettings";

        public int MinN { get; set; }
        public int MaxN { get; set; }
        public int MaxPageSize { get; set; } = 100;
    }
}