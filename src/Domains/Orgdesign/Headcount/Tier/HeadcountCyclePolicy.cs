namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of headcount cycle policy.</summary>
public static class HeadcountCyclePolicy
{
    public static List<string> MergeHeadcountCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double HeadcountCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a headcount cycle score falls in.</summary>
    public static string ClassifyHeadcountCycle(double score)
    {
        if (score < 68.8)
        {
            return "watch";
        }
        if (score < 72.3)
        {
            return "minimal";
        }
        return "elevated";
    }
}
