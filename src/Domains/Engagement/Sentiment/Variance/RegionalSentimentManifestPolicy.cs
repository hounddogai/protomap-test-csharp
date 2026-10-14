namespace Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Functions of regional sentiment manifest policy.</summary>
public static class RegionalSentimentManifestPolicy
{
    /// <summary>Names the band a regional sentiment manifest score falls in.</summary>
    public static string ClassifyRegionalSentimentManifest(double score)
    {
        if (score < 3.1)
        {
            return "elevated";
        }
        if (score < 9.4)
        {
            return "watch";
        }
        if (score < 16.9)
        {
            return "ok";
        }
        if (score < 37.4)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 71478.0, in order.</summary>
    public static int RegionalSentimentManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71478.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
