namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan variance policy.</summary>
public static class FloorPlanVariancePolicy
{
    public static int FloorPlanVarianceSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanVariance()
    {
        var policy = Retry.DefaultPolicy("floor-plan-variance");
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

    /// <summary>Counts how many amounts fit in a budget of 48164.0, in order.</summary>
    public static int FloorPlanVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48164.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
