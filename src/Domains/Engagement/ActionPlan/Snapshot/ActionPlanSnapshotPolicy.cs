namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan snapshot policy.</summary>
public static class ActionPlanSnapshotPolicy
{
    public static int ActionPlanSnapshotSpan(int start, int end)
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
        return span / 3600;
    }

    public static string ActionPlanSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a action plan snapshot code in its canonical APS- form.</summary>
    public static string NormalizeActionPlanSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
