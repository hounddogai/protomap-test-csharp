namespace Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Functions of sentiment allocation policy.</summary>
public static class SentimentAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 12838.0, in order.</summary>
    public static int SentimentAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12838.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a sentiment allocation score falls in.</summary>
    public static string ClassifySentimentAllocation(double score)
    {
        if (score < 5.3)
        {
            return "ok";
        }
        if (score < 82.2)
        {
            return "medium";
        }
        return "minimal";
    }
}
