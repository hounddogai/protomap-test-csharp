namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of access badge forecast policy.</summary>
public static class AccessBadgeForecastPolicy
{
    /// <summary>Names the band a access badge forecast score falls in.</summary>
    public static string ClassifyAccessBadgeForecast(double score)
    {
        if (score < 47.9)
        {
            return "low";
        }
        if (score < 65.2)
        {
            return "ok";
        }
        return "elevated";
    }

    public static List<string> MergeAccessBadgeForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double AccessBadgeForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AccessBadgeForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
