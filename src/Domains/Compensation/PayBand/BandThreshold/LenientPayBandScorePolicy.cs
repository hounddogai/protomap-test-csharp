namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Functions of lenient pay band score policy.</summary>
public static class LenientPayBandScorePolicy
{
    public static int LenientPayBandScoreSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 19997.0, in order.</summary>
    public static int LenientPayBandScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19997.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
