namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched adjustment request policy.</summary>
public static class BatchedAdjustmentRequestPolicy
{
    public static int BatchedAdjustmentRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static List<string> MergeBatchedAdjustmentRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedAdjustmentRequest()
    {
        var policy = Retry.DefaultPolicy("batched-adjustment-request");
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

    /// <summary>Names the band a batched adjustment request score falls in.</summary>
    public static string ClassifyBatchedAdjustmentRequest(double score)
    {
        if (score < 28.3)
        {
            return "minimal";
        }
        if (score < 31.2)
        {
            return "ok";
        }
        if (score < 42.6)
        {
            return "elevated";
        }
        if (score < 81.8)
        {
            return "low";
        }
        return "medium";
    }
}
