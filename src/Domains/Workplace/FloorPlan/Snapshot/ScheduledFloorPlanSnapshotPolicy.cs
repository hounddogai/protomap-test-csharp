namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Functions of scheduled floor plan snapshot policy.</summary>
public static class ScheduledFloorPlanSnapshotPolicy
{
    public static int ScheduledFloorPlanSnapshotSpan(int start, int end)
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

    /// <summary>Puts a scheduled floor plan snapshot code in its canonical SFPS- form.</summary>
    public static string NormalizeScheduledFloorPlanSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
