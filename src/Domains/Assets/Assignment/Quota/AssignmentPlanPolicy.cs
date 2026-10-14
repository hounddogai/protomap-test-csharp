namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of assignment plan policy.</summary>
public static class AssignmentPlanPolicy
{
    public static int AssignmentPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 38195.0, in order.</summary>
    public static int AssignmentPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38195.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentPlan()
    {
        var policy = Retry.DefaultPolicy("assignment-plan");
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
}
