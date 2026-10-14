namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Functions of legacy merit cycle review policy.</summary>
public static class LegacyMeritCycleReviewPolicy
{
    public static int LegacyMeritCycleReviewSpan(int start, int end)
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

    /// <summary>Puts a legacy merit cycle review code in its canonical LMCR- form.</summary>
    public static string NormalizeLegacyMeritCycleReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LMCR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LMCR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
