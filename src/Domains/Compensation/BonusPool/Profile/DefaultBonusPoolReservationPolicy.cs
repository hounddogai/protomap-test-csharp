namespace Acme.Workforce.Compensation.BonusPool.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default bonus pool reservation policy.</summary>
public static class DefaultBonusPoolReservationPolicy
{
    public static double DefaultBonusPoolReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 56030.0, in order.</summary>
    public static int DefaultBonusPoolReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56030.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
