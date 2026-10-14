namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Functions of feedback draft policy.</summary>
public static class FeedbackDraftPolicy
{
    /// <summary>Puts a feedback draft code in its canonical FD- form.</summary>
    public static string NormalizeFeedbackDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int FeedbackDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 72185.0, in order.</summary>
    public static int FeedbackDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72185.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
