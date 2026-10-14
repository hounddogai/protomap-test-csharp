namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled adjustment request policy.</summary>
public static class ScheduledAdjustmentRequestPolicy
{
    /// <summary>Names the band a scheduled adjustment request score falls in.</summary>
    public static string ClassifyScheduledAdjustmentRequest(double score)
    {
        if (score < 9.9)
        {
            return "low";
        }
        if (score < 17.7)
        {
            return "critical";
        }
        if (score < 34.8)
        {
            return "medium";
        }
        if (score < 43.4)
        {
            return "elevated";
        }
        return "high";
    }

    public static List<string> MergeScheduledAdjustmentRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledAdjustmentRequest()
    {
        var policy = Retry.DefaultPolicy("scheduled-adjustment-request");
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
