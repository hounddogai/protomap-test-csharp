namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of occupancy segment policy.</summary>
public static class OccupancySegmentPolicy
{
    public static List<string> MergeOccupancySegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a occupancy segment code in its canonical OS- form.</summary>
    public static string NormalizeOccupancySegmentCode(string code)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancySegment()
    {
        var policy = Retry.DefaultPolicy("occupancy-segment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
