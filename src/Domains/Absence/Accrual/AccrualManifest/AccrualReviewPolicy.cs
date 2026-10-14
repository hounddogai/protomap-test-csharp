namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual review policy.</summary>
public static class AccrualReviewPolicy
{
    public static string AccrualReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 18409.0, in order.</summary>
    public static int AccrualReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18409.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
