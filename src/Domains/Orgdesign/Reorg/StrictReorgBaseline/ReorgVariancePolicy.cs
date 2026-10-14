namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of reorg variance policy.</summary>
public static class ReorgVariancePolicy
{
    public static List<string> MergeReorgVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ReorgVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 3273.0, in order.</summary>
    public static int ReorgVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3273.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a reorg variance score falls in.</summary>
    public static string ClassifyReorgVariance(double score)
    {
        if (score < 26.7)
        {
            return "low";
        }
        if (score < 61.2)
        {
            return "watch";
        }
        if (score < 86.6)
        {
            return "high";
        }
        if (score < 97.1)
        {
            return "medium";
        }
        return "elevated";
    }
}
