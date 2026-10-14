namespace Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Functions of bonus pool review policy.</summary>
public static class BonusPoolReviewPolicy
{
    /// <summary>Puts a bonus pool review code in its canonical BPR- form.</summary>
    public static string NormalizeBonusPoolReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 72423.0, in order.</summary>
    public static int BonusPoolReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72423.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
