namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Functions of sentiment assignment policy.</summary>
public static class SentimentAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99526.0, in order.</summary>
    public static int SentimentAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99526.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SentimentAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
