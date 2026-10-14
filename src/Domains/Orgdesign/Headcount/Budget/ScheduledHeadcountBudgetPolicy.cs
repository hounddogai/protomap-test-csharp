namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled headcount budget policy.</summary>
public static class ScheduledHeadcountBudgetPolicy
{
    public static double ScheduledHeadcountBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledHeadcountBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeScheduledHeadcountBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled headcount budget score falls in.</summary>
    public static string ClassifyScheduledHeadcountBudget(double score)
    {
        if (score < 5.1)
        {
            return "elevated";
        }
        if (score < 9.2)
        {
            return "medium";
        }
        if (score < 22.1)
        {
            return "watch";
        }
        return "high";
    }
}
