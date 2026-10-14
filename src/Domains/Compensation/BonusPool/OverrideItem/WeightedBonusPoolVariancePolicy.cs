namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Functions of weighted bonus pool variance policy.</summary>
public static class WeightedBonusPoolVariancePolicy
{
    public static int WeightedBonusPoolVarianceSpan(int start, int end)
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

    /// <summary>Puts a weighted bonus pool variance code in its canonical WBPV- form.</summary>
    public static string NormalizeWeightedBonusPoolVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WBPV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WBPV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
