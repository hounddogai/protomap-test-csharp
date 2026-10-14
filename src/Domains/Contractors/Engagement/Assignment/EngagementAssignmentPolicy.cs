namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of engagement assignment policy.</summary>
public static class EngagementAssignmentPolicy
{
    public static List<string> MergeEngagementAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string EngagementAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a engagement assignment score falls in.</summary>
    public static string ClassifyEngagementAssignment(double score)
    {
        if (score < 1.8)
        {
            return "elevated";
        }
        if (score < 20.4)
        {
            return "ok";
        }
        if (score < 40.0)
        {
            return "critical";
        }
        if (score < 71.4)
        {
            return "minimal";
        }
        return "high";
    }
}
