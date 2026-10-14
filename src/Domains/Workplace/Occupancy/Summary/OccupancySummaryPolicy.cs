namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy summary policy.</summary>
public static class OccupancySummaryPolicy
{
    public static int OccupancySummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string OccupancySummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a occupancy summary code in its canonical OS- form.</summary>
    public static string NormalizeOccupancySummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
