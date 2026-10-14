namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy pay band rollup policy.</summary>
public static class LegacyPayBandRollupPolicy
{
    public static List<string> MergeLegacyPayBandRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy pay band rollup score falls in.</summary>
    public static string ClassifyLegacyPayBandRollup(double score)
    {
        if (score < 9.5)
        {
            return "low";
        }
        if (score < 97.7)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyPayBandRollup()
    {
        var policy = Retry.DefaultPolicy("legacy-pay-band-rollup");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
