namespace Acme.Workforce.Talent.CareerPath.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path adjustment policy.</summary>
public static class CareerPathAdjustmentPolicy
{
    public static int CareerPathAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static List<string> MergeCareerPathAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a career path adjustment score falls in.</summary>
    public static string ClassifyCareerPathAdjustment(double score)
    {
        if (score < 64.0)
        {
            return "watch";
        }
        if (score < 65.0)
        {
            return "medium";
        }
        if (score < 89.8)
        {
            return "high";
        }
        if (score < 90.7)
        {
            return "critical";
        }
        return "elevated";
    }
}
