namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Functions of lenient sentiment quota policy.</summary>
public static class LenientSentimentQuotaPolicy
{
    public static int LenientSentimentQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 54636.0, in order.</summary>
    public static int LenientSentimentQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54636.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
