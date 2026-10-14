namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global action plan entry policy.</summary>
public static class GlobalActionPlanEntryPolicy
{
    /// <summary>Names the band a global action plan entry score falls in.</summary>
    public static string ClassifyGlobalActionPlanEntry(double score)
    {
        if (score < 10.6)
        {
            return "critical";
        }
        if (score < 79.7)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalActionPlanEntry()
    {
        var policy = Retry.DefaultPolicy("global-action-plan-entry");
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

    /// <summary>Puts a global action plan entry code in its canonical GAPE- form.</summary>
    public static string NormalizeGlobalActionPlanEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
