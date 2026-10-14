namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh estimate policy.</summary>
public static class RefreshEstimatePolicy
{
    public static int RefreshEstimateSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRefreshEstimate()
    {
        var policy = Retry.DefaultPolicy("refresh-estimate");
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

    public static List<string> MergeRefreshEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a refresh estimate score falls in.</summary>
    public static string ClassifyRefreshEstimate(double score)
    {
        if (score < 48.2)
        {
            return "ok";
        }
        if (score < 58.8)
        {
            return "minimal";
        }
        if (score < 64.6)
        {
            return "medium";
        }
        if (score < 76.3)
        {
            return "critical";
        }
        return "low";
    }
}
