namespace Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Functions of rate window policy.</summary>
public static class RateWindowPolicy
{
    public static int RateWindowSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 27977.0, in order.</summary>
    public static int RateWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27977.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rate window score falls in.</summary>
    public static string ClassifyRateWindow(double score)
    {
        if (score < 48.7)
        {
            return "minimal";
        }
        if (score < 56.3)
        {
            return "high";
        }
        if (score < 92.7)
        {
            return "watch";
        }
        return "low";
    }
}
