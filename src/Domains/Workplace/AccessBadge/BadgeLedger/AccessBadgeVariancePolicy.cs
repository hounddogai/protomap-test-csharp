namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of access badge variance policy.</summary>
public static class AccessBadgeVariancePolicy
{
    /// <summary>Names the band a access badge variance score falls in.</summary>
    public static string ClassifyAccessBadgeVariance(double score)
    {
        if (score < 5.0)
        {
            return "medium";
        }
        if (score < 67.9)
        {
            return "ok";
        }
        if (score < 79.2)
        {
            return "low";
        }
        return "watch";
    }

    /// <summary>Puts a access badge variance code in its canonical ABV- form.</summary>
    public static string NormalizeAccessBadgeVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AccessBadgeVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string AccessBadgeVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
