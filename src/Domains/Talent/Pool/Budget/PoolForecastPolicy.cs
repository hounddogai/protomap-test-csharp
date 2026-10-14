namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool forecast policy.</summary>
public static class PoolForecastPolicy
{
    public static List<string> MergePoolForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string PoolForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a pool forecast code in its canonical PF- form.</summary>
    public static string NormalizePoolForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PoolForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
