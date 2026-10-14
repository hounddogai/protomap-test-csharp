namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of feedback ledger policy.</summary>
public static class FeedbackLedgerPolicy
{
    /// <summary>Puts a feedback ledger code in its canonical FL- form.</summary>
    public static string NormalizeFeedbackLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackLedger()
    {
        var policy = Retry.DefaultPolicy("feedback-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 85656.0, in order.</summary>
    public static int FeedbackLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85656.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a feedback ledger score falls in.</summary>
    public static string ClassifyFeedbackLedger(double score)
    {
        if (score < 22.8)
        {
            return "medium";
        }
        if (score < 68.7)
        {
            return "watch";
        }
        if (score < 80.4)
        {
            return "minimal";
        }
        return "critical";
    }
}
