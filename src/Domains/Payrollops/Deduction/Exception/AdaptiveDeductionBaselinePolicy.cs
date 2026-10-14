namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive deduction baseline policy.</summary>
public static class AdaptiveDeductionBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 94105.0, in order.</summary>
    public static int AdaptiveDeductionBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 94105.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdaptiveDeductionBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
