namespace Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Functions of manual retro pay tier policy.</summary>
public static class ManualRetroPayTierPolicy
{
    /// <summary>Puts a manual retro pay tier code in its canonical MRPT- form.</summary>
    public static string NormalizeManualRetroPayTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MRPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MRPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ManualRetroPayTierSpan(int start, int end)
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
        return span / 86400;
    }
}
