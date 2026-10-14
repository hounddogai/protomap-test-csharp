namespace Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Functions of pay band override policy.</summary>
public static class PayBandOverridePolicy
{
    public static int PayBandOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a pay band override code in its canonical PBO- form.</summary>
    public static string NormalizePayBandOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
