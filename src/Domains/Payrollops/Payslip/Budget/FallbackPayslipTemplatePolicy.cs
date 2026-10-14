namespace Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Functions of fallback payslip template policy.</summary>
public static class FallbackPayslipTemplatePolicy
{
    public static int FallbackPayslipTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 88503.0, in order.</summary>
    public static int FallbackPayslipTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88503.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
