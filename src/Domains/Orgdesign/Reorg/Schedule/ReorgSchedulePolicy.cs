namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Functions of reorg schedule policy.</summary>
public static class ReorgSchedulePolicy
{
    /// <summary>Puts a reorg schedule code in its canonical RS- form.</summary>
    public static string NormalizeReorgScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ReorgScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
