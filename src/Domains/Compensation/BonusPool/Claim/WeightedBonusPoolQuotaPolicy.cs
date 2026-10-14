namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted bonus pool quota policy.</summary>
public static class WeightedBonusPoolQuotaPolicy
{
    public static int WeightedBonusPoolQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a weighted bonus pool quota score falls in.</summary>
    public static string ClassifyWeightedBonusPoolQuota(double score)
    {
        if (score < 9.8)
        {
            return "high";
        }
        if (score < 54.9)
        {
            return "elevated";
        }
        if (score < 57.8)
        {
            return "medium";
        }
        return "minimal";
    }

    public static List<string> MergeWeightedBonusPoolQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 29895.0, in order.</summary>
    public static int WeightedBonusPoolQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29895.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
