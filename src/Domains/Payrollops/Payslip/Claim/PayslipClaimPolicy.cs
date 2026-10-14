namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip claim policy.</summary>
public static class PayslipClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 19611.0, in order.</summary>
    public static int PayslipClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19611.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipClaim()
    {
        var policy = Retry.DefaultPolicy("payslip-claim");
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
}
