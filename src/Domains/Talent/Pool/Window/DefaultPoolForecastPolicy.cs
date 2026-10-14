namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default pool forecast policy.</summary>
public static class DefaultPoolForecastPolicy
{
    /// <summary>Names the band a default pool forecast score falls in.</summary>
    public static string ClassifyDefaultPoolForecast(double score)
    {
        if (score < 43.9)
        {
            return "elevated";
        }
        if (score < 76.0)
        {
            return "high";
        }
        if (score < 89.7)
        {
            return "medium";
        }
        return "minimal";
    }

    public static double DefaultPoolForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeDefaultPoolForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
