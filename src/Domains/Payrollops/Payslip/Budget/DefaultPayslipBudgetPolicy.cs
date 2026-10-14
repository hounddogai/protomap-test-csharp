namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default payslip budget policy.</summary>
public static class DefaultPayslipBudgetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultPayslipBudget()
    {
        var policy = Retry.DefaultPolicy("default-payslip-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 19970.0, in order.</summary>
    public static int DefaultPayslipBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19970.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
