namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled leave request snapshot policy.</summary>
public static class ScheduledLeaveRequestSnapshotPolicy
{
    /// <summary>Puts a scheduled leave request snapshot code in its canonical SLRS- form.</summary>
    public static string NormalizeScheduledLeaveRequestSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SLRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SLRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled leave request snapshot score falls in.</summary>
    public static string ClassifyScheduledLeaveRequestSnapshot(double score)
    {
        if (score < 20.0)
        {
            return "low";
        }
        if (score < 21.1)
        {
            return "high";
        }
        return "watch";
    }

    public static List<string> MergeScheduledLeaveRequestSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ScheduledLeaveRequestSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
