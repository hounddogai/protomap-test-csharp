namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled occupancy snapshot policy.</summary>
public static class ScheduledOccupancySnapshotPolicy
{
    public static List<string> MergeScheduledOccupancySnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledOccupancySnapshot()
    {
        var policy = Retry.DefaultPolicy("scheduled-occupancy-snapshot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a scheduled occupancy snapshot score falls in.</summary>
    public static string ClassifyScheduledOccupancySnapshot(double score)
    {
        if (score < 1.9)
        {
            return "medium";
        }
        if (score < 89.9)
        {
            return "minimal";
        }
        return "ok";
    }
}
