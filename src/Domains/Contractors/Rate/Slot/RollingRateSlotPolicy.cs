namespace Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Functions of rolling rate slot policy.</summary>
public static class RollingRateSlotPolicy
{
    public static int RollingRateSlotSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a rolling rate slot code in its canonical RRS- form.</summary>
    public static string NormalizeRollingRateSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
