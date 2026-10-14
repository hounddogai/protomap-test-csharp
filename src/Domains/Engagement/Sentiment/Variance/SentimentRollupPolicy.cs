namespace Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Functions of sentiment rollup policy.</summary>
public static class SentimentRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 44429.0, in order.</summary>
    public static int SentimentRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44429.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a sentiment rollup score falls in.</summary>
    public static string ClassifySentimentRollup(double score)
    {
        if (score < 41.4)
        {
            return "watch";
        }
        if (score < 47.6)
        {
            return "critical";
        }
        if (score < 87.9)
        {
            return "elevated";
        }
        return "ok";
    }
}
