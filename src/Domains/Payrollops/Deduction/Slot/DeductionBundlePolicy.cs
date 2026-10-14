namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction bundle policy.</summary>
public static class DeductionBundlePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50128.0, in order.</summary>
    public static int DeductionBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50128.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string DeductionBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
