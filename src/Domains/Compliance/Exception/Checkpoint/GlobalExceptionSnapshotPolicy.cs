namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global exception snapshot policy.</summary>
public static class GlobalExceptionSnapshotPolicy
{
    public static List<string> MergeGlobalExceptionSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string GlobalExceptionSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
