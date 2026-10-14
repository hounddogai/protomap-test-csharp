namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending floor plan target policy.</summary>
public static class PendingFloorPlanTargetPolicy
{
    /// <summary>Puts a pending floor plan target code in its canonical PFPT- form.</summary>
    public static string NormalizePendingFloorPlanTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PFPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PFPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PendingFloorPlanTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 94584.0, in order.</summary>
    public static int PendingFloorPlanTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 94584.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingFloorPlanTarget()
    {
        var policy = Retry.DefaultPolicy("pending-floor-plan-target");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
