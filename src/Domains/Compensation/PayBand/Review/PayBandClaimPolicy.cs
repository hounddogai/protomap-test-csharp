namespace Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Functions of pay band claim policy.</summary>
public static class PayBandClaimPolicy
{
    /// <summary>Names the band a pay band claim score falls in.</summary>
    public static string ClassifyPayBandClaim(double score)
    {
        if (score < 7.2)
        {
            return "elevated";
        }
        if (score < 19.2)
        {
            return "high";
        }
        if (score < 37.4)
        {
            return "minimal";
        }
        if (score < 62.8)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 64510.0, in order.</summary>
    public static int PayBandClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64510.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PayBandClaimSpan(int start, int end)
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
        return span / 60;
    }
}
