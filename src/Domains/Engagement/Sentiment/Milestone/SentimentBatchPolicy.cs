namespace Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Functions of sentiment batch policy.</summary>
public static class SentimentBatchPolicy
{
    /// <summary>Puts a sentiment batch code in its canonical SB- form.</summary>
    public static string NormalizeSentimentBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 78951.0, in order.</summary>
    public static int SentimentBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78951.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SentimentBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
