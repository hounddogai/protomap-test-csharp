namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled garnishment group policy.</summary>
public static class ScheduledGarnishmentGroupPolicy
{
    public static List<string> MergeScheduledGarnishmentGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double ScheduledGarnishmentGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledGarnishmentGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a scheduled garnishment group score falls in.</summary>
    public static string ClassifyScheduledGarnishmentGroup(double score)
    {
        if (score < 13.1)
        {
            return "critical";
        }
        if (score < 49.1)
        {
            return "low";
        }
        if (score < 63.3)
        {
            return "high";
        }
        return "medium";
    }
}
