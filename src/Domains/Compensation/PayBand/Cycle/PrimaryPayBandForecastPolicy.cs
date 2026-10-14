namespace Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Functions of primary pay band forecast policy.</summary>
public static class PrimaryPayBandForecastPolicy
{
    public static int PrimaryPayBandForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 26714.0, in order.</summary>
    public static int PrimaryPayBandForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26714.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
