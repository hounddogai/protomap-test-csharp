namespace Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Functions of pay run review policy.</summary>
public static class PayRunReviewPolicy
{
    public static int PayRunReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a pay run review code in its canonical PRR- form.</summary>
    public static string NormalizePayRunReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 39076.0, in order.</summary>
    public static int PayRunReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39076.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
