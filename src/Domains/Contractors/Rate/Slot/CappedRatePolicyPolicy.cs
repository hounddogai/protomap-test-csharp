namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped rate policy policy.</summary>
public static class CappedRatePolicyPolicy
{
    /// <summary>Names the band a capped rate policy score falls in.</summary>
    public static string ClassifyCappedRatePolicy(double score)
    {
        if (score < 34.3)
        {
            return "ok";
        }
        if (score < 48.5)
        {
            return "minimal";
        }
        return "medium";
    }

    public static string CappedRatePolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 84919.0, in order.</summary>
    public static int CappedRatePolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84919.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeCappedRatePolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
