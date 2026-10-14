namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment review policy.</summary>
public static class AdjustmentReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 73056.0, in order.</summary>
    public static int AdjustmentReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73056.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdjustmentReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
