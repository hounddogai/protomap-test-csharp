namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted access badge exception policy.</summary>
public static class WeightedAccessBadgeExceptionPolicy
{
    public static double WeightedAccessBadgeExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string WeightedAccessBadgeExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a weighted access badge exception code in its canonical WABE- form.</summary>
    public static string NormalizeWeightedAccessBadgeExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WABE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WABE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted access badge exception score falls in.</summary>
    public static string ClassifyWeightedAccessBadgeException(double score)
    {
        if (score < 23.9)
        {
            return "medium";
        }
        if (score < 98.6)
        {
            return "ok";
        }
        return "minimal";
    }
}
