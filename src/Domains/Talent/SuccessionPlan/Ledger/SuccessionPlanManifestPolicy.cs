namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of succession plan manifest policy.</summary>
public static class SuccessionPlanManifestPolicy
{
    public static int SuccessionPlanManifestSpan(int start, int end)
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

    /// <summary>Names the band a succession plan manifest score falls in.</summary>
    public static string ClassifySuccessionPlanManifest(double score)
    {
        if (score < 46.0)
        {
            return "minimal";
        }
        if (score < 58.3)
        {
            return "ok";
        }
        if (score < 85.1)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSuccessionPlanManifest()
    {
        var policy = Retry.DefaultPolicy("succession-plan-manifest");
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

    public static string SuccessionPlanManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
