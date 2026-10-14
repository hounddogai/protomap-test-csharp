namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Functions of seasonal retro pay review policy.</summary>
public static class SeasonalRetroPayReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 56527.0, in order.</summary>
    public static int SeasonalRetroPayReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56527.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SeasonalRetroPayReviewSpan(int start, int end)
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
