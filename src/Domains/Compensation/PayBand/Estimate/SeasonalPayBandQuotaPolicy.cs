namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal pay band quota policy.</summary>
public static class SeasonalPayBandQuotaPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31739.0, in order.</summary>
    public static int SeasonalPayBandQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31739.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double SeasonalPayBandQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
