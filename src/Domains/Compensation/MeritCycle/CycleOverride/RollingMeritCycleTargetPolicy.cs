namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling merit cycle target policy.</summary>
public static class RollingMeritCycleTargetPolicy
{
    public static List<string> MergeRollingMeritCycleTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rolling merit cycle target score falls in.</summary>
    public static string ClassifyRollingMeritCycleTarget(double score)
    {
        if (score < 12.6)
        {
            return "watch";
        }
        if (score < 57.2)
        {
            return "high";
        }
        return "critical";
    }

    public static double RollingMeritCycleTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
