namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Functions of batched deduction budget policy.</summary>
public static class BatchedDeductionBudgetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 38179.0, in order.</summary>
    public static int BatchedDeductionBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38179.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int BatchedDeductionBudgetSpan(int start, int end)
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
