namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip window policy.</summary>
public static class PayslipWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 37306.0, in order.</summary>
    public static int PayslipWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37306.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipWindow()
    {
        var policy = Retry.DefaultPolicy("payslip-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
