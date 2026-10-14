namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual estimate policy.</summary>
public static class AccrualEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 20153.0, in order.</summary>
    public static int AccrualEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20153.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AccrualEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
