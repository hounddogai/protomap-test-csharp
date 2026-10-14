namespace Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>Functions of pay run digest policy.</summary>
public static class PayRunDigestPolicy
{
    public static int PayRunDigestSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a pay run digest score falls in.</summary>
    public static string ClassifyPayRunDigest(double score)
    {
        if (score < 4.4)
        {
            return "low";
        }
        if (score < 63.4)
        {
            return "minimal";
        }
        if (score < 78.9)
        {
            return "critical";
        }
        if (score < 83.3)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 54536.0, in order.</summary>
    public static int PayRunDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54536.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
