namespace Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Functions of merit cycle segment policy.</summary>
public static class MeritCycleSegmentPolicy
{
    public static int MeritCycleSegmentSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a merit cycle segment code in its canonical MCS- form.</summary>
    public static string NormalizeMeritCycleSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
