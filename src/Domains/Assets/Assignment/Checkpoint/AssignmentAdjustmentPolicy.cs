namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of assignment adjustment policy.</summary>
public static class AssignmentAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26495.0, in order.</summary>
    public static int AssignmentAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26495.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentAdjustment()
    {
        var policy = Retry.DefaultPolicy("assignment-adjustment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
