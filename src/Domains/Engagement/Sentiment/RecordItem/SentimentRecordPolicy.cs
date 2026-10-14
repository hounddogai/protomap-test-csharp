namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment record policy.</summary>
public static class SentimentRecordPolicy
{
    public static string SentimentRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 77149.0, in order.</summary>
    public static int SentimentRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77149.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
