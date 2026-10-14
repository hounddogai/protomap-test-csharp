namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment ledger policy.</summary>
public static class SentimentLedgerPolicy
{
    public static string SentimentLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double SentimentLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a sentiment ledger score falls in.</summary>
    public static string ClassifySentimentLedger(double score)
    {
        if (score < 25.4)
        {
            return "watch";
        }
        if (score < 41.1)
        {
            return "medium";
        }
        if (score < 59.3)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 68908.0, in order.</summary>
    public static int SentimentLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68908.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
