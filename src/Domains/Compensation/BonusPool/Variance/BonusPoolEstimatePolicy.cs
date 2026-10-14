namespace Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Functions of bonus pool estimate policy.</summary>
public static class BonusPoolEstimatePolicy
{
    /// <summary>Puts a bonus pool estimate code in its canonical BPE- form.</summary>
    public static string NormalizeBonusPoolEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BonusPoolEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}
