namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle item policy.</summary>
public static class MeritCycleItemPolicy
{
    public static int MeritCycleItemSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a merit cycle item score falls in.</summary>
    public static string ClassifyMeritCycleItem(double score)
    {
        if (score < 6.5)
        {
            return "critical";
        }
        if (score < 84.0)
        {
            return "medium";
        }
        return "ok";
    }

    public static string MeritCycleItemBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeMeritCycleItemLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
