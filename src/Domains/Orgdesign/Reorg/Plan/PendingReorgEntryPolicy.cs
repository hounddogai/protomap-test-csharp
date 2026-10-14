namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending reorg entry policy.</summary>
public static class PendingReorgEntryPolicy
{
    public static double PendingReorgEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.07;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PendingReorgEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a pending reorg entry code in its canonical PRE- form.</summary>
    public static string NormalizePendingReorgEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a pending reorg entry score falls in.</summary>
    public static string ClassifyPendingReorgEntry(double score)
    {
        if (score < 28.1)
        {
            return "minimal";
        }
        if (score < 41.2)
        {
            return "critical";
        }
        if (score < 54.8)
        {
            return "high";
        }
        if (score < 75.3)
        {
            return "watch";
        }
        return "medium";
    }
}
