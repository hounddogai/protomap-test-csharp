namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of accrual threshold policy.</summary>
public static class AccrualThresholdPolicy
{
    public static List<string> MergeAccrualThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a accrual threshold score falls in.</summary>
    public static string ClassifyAccrualThreshold(double score)
    {
        if (score < 59.8)
        {
            return "critical";
        }
        if (score < 89.5)
        {
            return "low";
        }
        return "medium";
    }

    public static double AccrualThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
