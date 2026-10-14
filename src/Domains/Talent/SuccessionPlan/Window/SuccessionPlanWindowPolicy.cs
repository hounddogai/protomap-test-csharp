namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan window policy.</summary>
public static class SuccessionPlanWindowPolicy
{
    public static double SuccessionPlanWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeSuccessionPlanWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a succession plan window score falls in.</summary>
    public static string ClassifySuccessionPlanWindow(double score)
    {
        if (score < 47.1)
        {
            return "watch";
        }
        if (score < 92.3)
        {
            return "minimal";
        }
        return "critical";
    }
}
