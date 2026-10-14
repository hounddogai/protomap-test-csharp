namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of action plan cycle policy.</summary>
public static class ActionPlanCyclePolicy
{
    public static double ActionPlanCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a action plan cycle score falls in.</summary>
    public static string ClassifyActionPlanCycle(double score)
    {
        if (score < 12.0)
        {
            return "high";
        }
        if (score < 19.6)
        {
            return "elevated";
        }
        if (score < 38.4)
        {
            return "medium";
        }
        return "ok";
    }

    public static int ActionPlanCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeActionPlanCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
