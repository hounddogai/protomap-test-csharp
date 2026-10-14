namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of reorg policy policy.</summary>
public static class ReorgPolicyPolicy
{
    public static double ReorgPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a reorg policy score falls in.</summary>
    public static string ClassifyReorgPolicy(double score)
    {
        if (score < 44.7)
        {
            return "medium";
        }
        if (score < 44.9)
        {
            return "minimal";
        }
        if (score < 98.1)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReorgPolicy()
    {
        var policy = Retry.DefaultPolicy("reorg-policy");
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
