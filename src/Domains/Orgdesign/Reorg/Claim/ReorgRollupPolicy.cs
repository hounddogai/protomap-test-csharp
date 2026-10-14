namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of reorg rollup policy.</summary>
public static class ReorgRollupPolicy
{
    /// <summary>Names the band a reorg rollup score falls in.</summary>
    public static string ClassifyReorgRollup(double score)
    {
        if (score < 15.0)
        {
            return "watch";
        }
        if (score < 73.6)
        {
            return "elevated";
        }
        if (score < 76.1)
        {
            return "high";
        }
        return "minimal";
    }

    public static int ReorgRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReorgRollup()
    {
        var policy = Retry.DefaultPolicy("reorg-rollup");
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

    public static List<string> MergeReorgRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
