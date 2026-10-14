namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band allocation policy.</summary>
public static class PayBandAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 79204.0, in order.</summary>
    public static int PayBandAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79204.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PayBandAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PayBandAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
