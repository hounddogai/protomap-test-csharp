namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Functions of retro pay threshold policy.</summary>
public static class RetroPayThresholdPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 80464.0, in order.</summary>
    public static int RetroPayThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80464.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RetroPayThresholdSpan(int start, int end)
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
