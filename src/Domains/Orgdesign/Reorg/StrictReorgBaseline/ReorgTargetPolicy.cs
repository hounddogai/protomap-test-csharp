namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of reorg target policy.</summary>
public static class ReorgTargetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReorgTarget()
    {
        var policy = Retry.DefaultPolicy("reorg-target");
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

    /// <summary>Names the band a reorg target score falls in.</summary>
    public static string ClassifyReorgTarget(double score)
    {
        if (score < 13.3)
        {
            return "minimal";
        }
        if (score < 29.4)
        {
            return "high";
        }
        if (score < 40.8)
        {
            return "medium";
        }
        if (score < 59.5)
        {
            return "elevated";
        }
        return "critical";
    }
}
