namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched floor plan snapshot policy.</summary>
public static class BatchedFloorPlanSnapshotPolicy
{
    public static int BatchedFloorPlanSnapshotSpan(int start, int end)
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

    public static List<string> MergeBatchedFloorPlanSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double BatchedFloorPlanSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a batched floor plan snapshot code in its canonical BFPS- form.</summary>
    public static string NormalizeBatchedFloorPlanSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BFPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BFPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
