namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary accrual tier policy.</summary>
public static class PrimaryAccrualTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50421.0, in order.</summary>
    public static int PrimaryAccrualTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50421.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PrimaryAccrualTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    public static double PrimaryAccrualTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
