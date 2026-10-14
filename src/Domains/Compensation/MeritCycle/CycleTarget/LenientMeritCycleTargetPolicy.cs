namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient merit cycle target policy.</summary>
public static class LenientMeritCycleTargetPolicy
{
    /// <summary>Names the band a lenient merit cycle target score falls in.</summary>
    public static string ClassifyLenientMeritCycleTarget(double score)
    {
        if (score < 6.5)
        {
            return "low";
        }
        if (score < 61.2)
        {
            return "medium";
        }
        if (score < 86.1)
        {
            return "high";
        }
        return "elevated";
    }

    public static List<string> MergeLenientMeritCycleTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
