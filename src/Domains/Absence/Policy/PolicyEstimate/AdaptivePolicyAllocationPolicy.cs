namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive policy allocation policy.</summary>
public static class AdaptivePolicyAllocationPolicy
{
    /// <summary>Names the band a adaptive policy allocation score falls in.</summary>
    public static string ClassifyAdaptivePolicyAllocation(double score)
    {
        if (score < 11.7)
        {
            return "ok";
        }
        if (score < 38.3)
        {
            return "watch";
        }
        if (score < 65.9)
        {
            return "low";
        }
        return "minimal";
    }

    public static List<string> MergeAdaptivePolicyAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 38354.0, in order.</summary>
    public static int AdaptivePolicyAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38354.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
