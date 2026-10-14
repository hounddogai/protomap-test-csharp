namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Functions of regional reorg policy policy.</summary>
public static class RegionalReorgPolicyPolicy
{
    public static int RegionalReorgPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a regional reorg policy code in its canonical RRP- form.</summary>
    public static string NormalizeRegionalReorgPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
