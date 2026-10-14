namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate mapping policy.</summary>
public static class RateMappingPolicy
{
    public static string RateMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a rate mapping score falls in.</summary>
    public static string ClassifyRateMapping(double score)
    {
        if (score < 10.4)
        {
            return "high";
        }
        if (score < 35.8)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Puts a rate mapping code in its canonical RM- form.</summary>
    public static string NormalizeRateMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RateMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
