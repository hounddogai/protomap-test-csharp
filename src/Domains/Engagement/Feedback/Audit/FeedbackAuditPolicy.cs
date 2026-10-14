namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback audit policy.</summary>
public static class FeedbackAuditPolicy
{
    /// <summary>Puts a feedback audit code in its canonical FA- form.</summary>
    public static string NormalizeFeedbackAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 77243.0, in order.</summary>
    public static int FeedbackAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77243.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FeedbackAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
