namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional accrual summary policy.</summary>
public static class RegionalAccrualSummaryPolicy
{
    /// <summary>Puts a regional accrual summary code in its canonical RAS- form.</summary>
    public static string NormalizeRegionalAccrualSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RegionalAccrualSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RegionalAccrualSummarySpan(int start, int end)
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
        return span / 60;
    }
}
