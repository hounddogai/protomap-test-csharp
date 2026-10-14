namespace Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Functions of scheduled pay band tier policy.</summary>
public static class ScheduledPayBandTierPolicy
{
    public static int ScheduledPayBandTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 15449.0, in order.</summary>
    public static int ScheduledPayBandTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15449.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
