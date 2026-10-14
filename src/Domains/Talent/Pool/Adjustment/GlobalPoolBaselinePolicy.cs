namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global pool baseline policy.</summary>
public static class GlobalPoolBaselinePolicy
{
    /// <summary>Names the band a global pool baseline score falls in.</summary>
    public static string ClassifyGlobalPoolBaseline(double score)
    {
        if (score < 2.4)
        {
            return "high";
        }
        if (score < 30.0)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalPoolBaseline()
    {
        var policy = Retry.DefaultPolicy("global-pool-baseline");
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
