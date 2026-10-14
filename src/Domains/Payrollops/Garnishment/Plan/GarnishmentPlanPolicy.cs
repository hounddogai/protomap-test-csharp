namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of garnishment plan policy.</summary>
public static class GarnishmentPlanPolicy
{
    public static int GarnishmentPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGarnishmentPlan()
    {
        var policy = Retry.DefaultPolicy("garnishment-plan");
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
