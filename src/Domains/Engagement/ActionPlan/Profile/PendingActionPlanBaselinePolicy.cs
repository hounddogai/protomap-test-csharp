namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending action plan baseline policy.</summary>
public static class PendingActionPlanBaselinePolicy
{
    public static double PendingActionPlanBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingActionPlanBaseline()
    {
        var policy = Retry.DefaultPolicy("pending-action-plan-baseline");
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
