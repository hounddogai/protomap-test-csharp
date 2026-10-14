namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Functions of occupancy entry policy.</summary>
public static class OccupancyEntryPolicy
{
    /// <summary>Names the band a occupancy entry score falls in.</summary>
    public static string ClassifyOccupancyEntry(double score)
    {
        if (score < 9.2)
        {
            return "elevated";
        }
        if (score < 16.6)
        {
            return "low";
        }
        if (score < 21.7)
        {
            return "high";
        }
        if (score < 41.1)
        {
            return "ok";
        }
        return "minimal";
    }

    public static int OccupancyEntrySpan(int start, int end)
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

    /// <summary>Puts a occupancy entry code in its canonical OE- form.</summary>
    public static string NormalizeOccupancyEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
