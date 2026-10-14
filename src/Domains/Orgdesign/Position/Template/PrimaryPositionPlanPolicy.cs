namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary position plan policy.</summary>
public static class PrimaryPositionPlanPolicy
{
    public static List<string> MergePrimaryPositionPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PrimaryPositionPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary position plan score falls in.</summary>
    public static string ClassifyPrimaryPositionPlan(double score)
    {
        if (score < 22.1)
        {
            return "high";
        }
        if (score < 74.7)
        {
            return "watch";
        }
        return "critical";
    }
}
