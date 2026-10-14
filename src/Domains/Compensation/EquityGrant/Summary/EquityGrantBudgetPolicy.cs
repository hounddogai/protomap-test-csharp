namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of equity grant budget policy.</summary>
public static class EquityGrantBudgetPolicy
{
    public static int EquityGrantBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string EquityGrantBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeEquityGrantBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a equity grant budget score falls in.</summary>
    public static string ClassifyEquityGrantBudget(double score)
    {
        if (score < 14.4)
        {
            return "elevated";
        }
        if (score < 19.3)
        {
            return "minimal";
        }
        if (score < 60.6)
        {
            return "low";
        }
        return "ok";
    }
}
