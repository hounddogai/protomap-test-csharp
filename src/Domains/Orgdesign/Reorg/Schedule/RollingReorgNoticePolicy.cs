namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Functions of rolling reorg notice policy.</summary>
public static class RollingReorgNoticePolicy
{
    /// <summary>Puts a rolling reorg notice code in its canonical RRN- form.</summary>
    public static string NormalizeRollingReorgNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RollingReorgNoticeSpan(int start, int end)
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
