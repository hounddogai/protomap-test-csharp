namespace Acme.Workforce.Assets.Refresh.Review;

/// <summary>Functions of refresh review policy.</summary>
public static class RefreshReviewPolicy
{
    public static int RefreshReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 55971.0, in order.</summary>
    public static int RefreshReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55971.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
