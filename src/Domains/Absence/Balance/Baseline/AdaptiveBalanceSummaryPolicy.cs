namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive balance summary policy.</summary>
public static class AdaptiveBalanceSummaryPolicy
{
    /// <summary>Puts a adaptive balance summary code in its canonical ABS- form.</summary>
    public static string NormalizeAdaptiveBalanceSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveBalanceSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static double AdaptiveBalanceSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
