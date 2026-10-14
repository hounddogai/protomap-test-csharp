namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band forecast policy.</summary>
public static class PayBandForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 93855.0, in order.</summary>
    public static int PayBandForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93855.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PayBandForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.72;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
