namespace Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Functions of training due review policy.</summary>
public static class TrainingDueReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 76108.0, in order.</summary>
    public static int TrainingDueReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76108.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TrainingDueReviewSpan(int start, int end)
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
        return span / 86400;
    }
}
