namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary adjustment assignment policy.</summary>
public static class PrimaryAdjustmentAssignmentPolicy
{
    public static double PrimaryAdjustmentAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePrimaryAdjustmentAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a primary adjustment assignment score falls in.</summary>
    public static string ClassifyPrimaryAdjustmentAssignment(double score)
    {
        if (score < 37.3)
        {
            return "watch";
        }
        if (score < 44.0)
        {
            return "critical";
        }
        if (score < 96.8)
        {
            return "low";
        }
        return "medium";
    }
}
