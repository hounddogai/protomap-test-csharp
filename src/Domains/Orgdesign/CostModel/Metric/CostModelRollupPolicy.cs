namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of cost model rollup policy.</summary>
public static class CostModelRollupPolicy
{
    public static List<string> MergeCostModelRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 31119.0, in order.</summary>
    public static int CostModelRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31119.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a cost model rollup score falls in.</summary>
    public static string ClassifyCostModelRollup(double score)
    {
        if (score < 56.6)
        {
            return "watch";
        }
        if (score < 94.3)
        {
            return "ok";
        }
        if (score < 95.8)
        {
            return "critical";
        }
        return "low";
    }
}
