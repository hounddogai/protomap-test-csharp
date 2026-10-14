namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict visitor slot policy.</summary>
public static class StrictVisitorSlotPolicy
{
    public static List<string> MergeStrictVisitorSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a strict visitor slot score falls in.</summary>
    public static string ClassifyStrictVisitorSlot(double score)
    {
        if (score < 63.0)
        {
            return "critical";
        }
        if (score < 80.9)
        {
            return "ok";
        }
        if (score < 89.5)
        {
            return "low";
        }
        return "elevated";
    }
}
