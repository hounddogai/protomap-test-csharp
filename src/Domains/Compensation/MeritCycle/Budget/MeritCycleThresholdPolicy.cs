namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle threshold policy.</summary>
public static class MeritCycleThresholdPolicy
{
    public static List<string> MergeMeritCycleThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string MeritCycleThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a merit cycle threshold score falls in.</summary>
    public static string ClassifyMeritCycleThreshold(double score)
    {
        if (score < 67.1)
        {
            return "medium";
        }
        if (score < 70.9)
        {
            return "minimal";
        }
        return "high";
    }
}
