namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of refresh tier policy.</summary>
public static class RefreshTierPolicy
{
    public static double RefreshTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 44010.0, in order.</summary>
    public static int RefreshTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44010.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
