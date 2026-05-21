namespace NutriPlanner.Models
{
    public class RecetteStat
    {
        public string Label { get; set; } = "";
        public double Valeur { get; set; }
    }

    public class RecetteCountStat
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
    }
}