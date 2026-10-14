namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of return plan adjustment policy.</summary>
public static class ReturnPlanAdjustmentPolicy
{
    public static int ReturnPlanAdjustmentSpan(int start, int end)
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
    public static int ProbeReturnPlanAdjustment()
    {
        var policy = Retry.DefaultPolicy("return-plan-adjustment");
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

    /// <summary>Counts how many amounts fit in a budget of 26638.0, in order.</summary>
    public static int ReturnPlanAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26638.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
