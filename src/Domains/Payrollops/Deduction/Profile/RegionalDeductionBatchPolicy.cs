namespace Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Functions of regional deduction batch policy.</summary>
public static class RegionalDeductionBatchPolicy
{
    public static int RegionalDeductionBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 48249.0, in order.</summary>
    public static int RegionalDeductionBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48249.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
