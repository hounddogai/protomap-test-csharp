namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback refresh policy policy.</summary>
public static class FallbackRefreshPolicyPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackRefreshPolicy()
    {
        var policy = Retry.DefaultPolicy("fallback-refresh-policy");
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

    /// <summary>Names the band a fallback refresh policy score falls in.</summary>
    public static string ClassifyFallbackRefreshPolicy(double score)
    {
        if (score < 2.3)
        {
            return "high";
        }
        if (score < 23.2)
        {
            return "critical";
        }
        if (score < 45.3)
        {
            return "elevated";
        }
        if (score < 54.1)
        {
            return "watch";
        }
        return "minimal";
    }
}
