namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending sentiment variance policy.</summary>
public static class PendingSentimentVariancePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 82467.0, in order.</summary>
    public static int PendingSentimentVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82467.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending sentiment variance score falls in.</summary>
    public static string ClassifyPendingSentimentVariance(double score)
    {
        if (score < 7.4)
        {
            return "high";
        }
        if (score < 57.6)
        {
            return "minimal";
        }
        if (score < 67.8)
        {
            return "low";
        }
        return "medium";
    }

    public static int PendingSentimentVarianceSpan(int start, int end)
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
        return span / 60;
    }

    public static string PendingSentimentVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
