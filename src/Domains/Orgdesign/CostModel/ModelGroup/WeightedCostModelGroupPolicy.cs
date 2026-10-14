namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>Functions of weighted cost model group policy.</summary>
public static class WeightedCostModelGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 56482.0, in order.</summary>
    public static int WeightedCostModelGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56482.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a weighted cost model group score falls in.</summary>
    public static string ClassifyWeightedCostModelGroup(double score)
    {
        if (score < 41.5)
        {
            return "medium";
        }
        if (score < 97.2)
        {
            return "ok";
        }
        return "minimal";
    }
}
