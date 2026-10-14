namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of reorg claim policy.</summary>
public static class ReorgClaimPolicy
{
    /// <summary>Names the band a reorg claim score falls in.</summary>
    public static string ClassifyReorgClaim(double score)
    {
        if (score < 52.8)
        {
            return "minimal";
        }
        if (score < 61.8)
        {
            return "medium";
        }
        if (score < 72.1)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReorgClaim()
    {
        var policy = Retry.DefaultPolicy("reorg-claim");
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

    public static double ReorgClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
