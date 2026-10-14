namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling cost model preference policy.</summary>
public static class RollingCostModelPreferencePolicy
{
    /// <summary>Names the band a rolling cost model preference score falls in.</summary>
    public static string ClassifyRollingCostModelPreference(double score)
    {
        if (score < 20.1)
        {
            return "elevated";
        }
        if (score < 62.8)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 39509.0, in order.</summary>
    public static int RollingCostModelPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39509.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RollingCostModelPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
