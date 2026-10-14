namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Functions of cost model override policy.</summary>
public static class CostModelOverridePolicy
{
    /// <summary>Names the band a cost model override score falls in.</summary>
    public static string ClassifyCostModelOverride(double score)
    {
        if (score < 39.6)
        {
            return "elevated";
        }
        if (score < 48.6)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 81296.0, in order.</summary>
    public static int CostModelOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81296.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
