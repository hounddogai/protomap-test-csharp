namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual balance digest policy.</summary>
public static class ManualBalanceDigestPolicy
{
    public static double ManualBalanceDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a manual balance digest code in its canonical MBD- form.</summary>
    public static string NormalizeManualBalanceDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MBD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MBD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ManualBalanceDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a manual balance digest score falls in.</summary>
    public static string ClassifyManualBalanceDigest(double score)
    {
        if (score < 6.3)
        {
            return "low";
        }
        if (score < 71.5)
        {
            return "high";
        }
        if (score < 80.1)
        {
            return "watch";
        }
        if (score < 90.7)
        {
            return "elevated";
        }
        return "critical";
    }
}
