namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan assignment policy.</summary>
public static class SuccessionPlanAssignmentPolicy
{
    public static string SuccessionPlanAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeSuccessionPlanAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a succession plan assignment score falls in.</summary>
    public static string ClassifySuccessionPlanAssignment(double score)
    {
        if (score < 41.2)
        {
            return "high";
        }
        if (score < 52.5)
        {
            return "elevated";
        }
        if (score < 77.2)
        {
            return "minimal";
        }
        return "ok";
    }
}
