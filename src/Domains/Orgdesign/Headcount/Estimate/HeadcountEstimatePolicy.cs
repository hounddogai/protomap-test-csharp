namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

/// <summary>Functions of headcount estimate policy.</summary>
public static class HeadcountEstimatePolicy
{
    /// <summary>Names the band a headcount estimate score falls in.</summary>
    public static string ClassifyHeadcountEstimate(double score)
    {
        if (score < 8.2)
        {
            return "elevated";
        }
        if (score < 9.1)
        {
            return "watch";
        }
        if (score < 16.8)
        {
            return "high";
        }
        if (score < 45.7)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 50252.0, in order.</summary>
    public static int HeadcountEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50252.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
