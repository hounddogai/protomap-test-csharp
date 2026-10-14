namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict bonus pool group policy.</summary>
public static class StrictBonusPoolGroupPolicy
{
    public static double StrictBonusPoolGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 92688.0, in order.</summary>
    public static int StrictBonusPoolGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92688.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
