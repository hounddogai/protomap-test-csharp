namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of action plan bundle policy.</summary>
public static class ActionPlanBundlePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeActionPlanBundle()
    {
        var policy = Retry.DefaultPolicy("action-plan-bundle");
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

    /// <summary>Names the band a action plan bundle score falls in.</summary>
    public static string ClassifyActionPlanBundle(double score)
    {
        if (score < 21.9)
        {
            return "ok";
        }
        if (score < 28.7)
        {
            return "elevated";
        }
        if (score < 65.7)
        {
            return "low";
        }
        return "medium";
    }
}
