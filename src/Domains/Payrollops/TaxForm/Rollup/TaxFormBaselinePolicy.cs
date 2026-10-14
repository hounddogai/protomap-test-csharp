namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Functions of tax form baseline policy.</summary>
public static class TaxFormBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 70789.0, in order.</summary>
    public static int TaxFormBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70789.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TaxFormBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
