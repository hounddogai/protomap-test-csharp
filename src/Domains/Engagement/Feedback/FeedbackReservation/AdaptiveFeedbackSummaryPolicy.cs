namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive feedback summary policy.</summary>
public static class AdaptiveFeedbackSummaryPolicy
{
    /// <summary>Puts a adaptive feedback summary code in its canonical AFS- form.</summary>
    public static string NormalizeAdaptiveFeedbackSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AFS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AFS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveFeedbackSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 33673.0, in order.</summary>
    public static int AdaptiveFeedbackSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33673.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveFeedbackSummary()
    {
        var policy = Retry.DefaultPolicy("adaptive-feedback-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
