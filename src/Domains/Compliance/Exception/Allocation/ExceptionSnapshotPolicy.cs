namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception snapshot policy.</summary>
public static class ExceptionSnapshotPolicy
{
    /// <summary>Names the band a exception snapshot score falls in.</summary>
    public static string ClassifyExceptionSnapshot(double score)
    {
        if (score < 30.7)
        {
            return "low";
        }
        if (score < 55.9)
        {
            return "minimal";
        }
        if (score < 74.1)
        {
            return "watch";
        }
        if (score < 86.7)
        {
            return "medium";
        }
        return "high";
    }

    public static List<string> MergeExceptionSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
