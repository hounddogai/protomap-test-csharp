namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled return plan target policy.</summary>
public static class ScheduledReturnPlanTargetPolicy
{
    public static double ScheduledReturnPlanTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeScheduledReturnPlanTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled return plan target score falls in.</summary>
    public static string ClassifyScheduledReturnPlanTarget(double score)
    {
        if (score < 7.3)
        {
            return "watch";
        }
        if (score < 29.0)
        {
            return "high";
        }
        return "ok";
    }
}
