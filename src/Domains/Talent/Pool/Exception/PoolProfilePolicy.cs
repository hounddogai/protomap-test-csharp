namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool profile policy.</summary>
public static class PoolProfilePolicy
{
    public static int PoolProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a pool profile score falls in.</summary>
    public static string ClassifyPoolProfile(double score)
    {
        if (score < 2.3)
        {
            return "low";
        }
        if (score < 41.0)
        {
            return "critical";
        }
        if (score < 53.4)
        {
            return "medium";
        }
        if (score < 69.9)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Puts a pool profile code in its canonical PP- form.</summary>
    public static string NormalizePoolProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PoolProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
