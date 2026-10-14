namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary retro pay audit policy.</summary>
public static class PrimaryRetroPayAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10442.0, in order.</summary>
    public static int PrimaryRetroPayAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10442.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PrimaryRetroPayAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryRetroPayAudit()
    {
        var policy = Retry.DefaultPolicy("primary-retro-pay-audit");
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
