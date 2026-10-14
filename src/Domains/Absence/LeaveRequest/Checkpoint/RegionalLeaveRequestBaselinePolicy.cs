namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional leave request baseline policy.</summary>
public static class RegionalLeaveRequestBaselinePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalLeaveRequestBaseline()
    {
        var policy = Retry.DefaultPolicy("regional-leave-request-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 88707.0, in order.</summary>
    public static int RegionalLeaveRequestBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88707.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RegionalLeaveRequestBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
