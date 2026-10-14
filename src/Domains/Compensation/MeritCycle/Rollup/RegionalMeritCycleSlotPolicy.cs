namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional merit cycle slot policy.</summary>
public static class RegionalMeritCycleSlotPolicy
{
    public static int RegionalMeritCycleSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a regional merit cycle slot score falls in.</summary>
    public static string ClassifyRegionalMeritCycleSlot(double score)
    {
        if (score < 50.6)
        {
            return "high";
        }
        if (score < 90.7)
        {
            return "low";
        }
        return "critical";
    }

    public static double RegionalMeritCycleSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeRegionalMeritCycleSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
