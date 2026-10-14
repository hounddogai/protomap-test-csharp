namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of occupancy forecast policy.</summary>
public static class OccupancyForecastPolicy
{
    public static double OccupancyForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a occupancy forecast code in its canonical OF- form.</summary>
    public static string NormalizeOccupancyForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeOccupancyForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
