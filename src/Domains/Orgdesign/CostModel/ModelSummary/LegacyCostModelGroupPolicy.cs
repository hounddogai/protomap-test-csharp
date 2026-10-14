namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy cost model group policy.</summary>
public static class LegacyCostModelGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 22384.0, in order.</summary>
    public static int LegacyCostModelGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22384.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LegacyCostModelGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
