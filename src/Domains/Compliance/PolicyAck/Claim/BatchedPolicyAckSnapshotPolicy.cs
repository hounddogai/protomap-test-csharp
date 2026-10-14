namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched policy ack snapshot policy.</summary>
public static class BatchedPolicyAckSnapshotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedPolicyAckSnapshot()
    {
        var policy = Retry.DefaultPolicy("batched-policy-ack-snapshot");
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

    public static double BatchedPolicyAckSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a batched policy ack snapshot score falls in.</summary>
    public static string ClassifyBatchedPolicyAckSnapshot(double score)
    {
        if (score < 41.8)
        {
            return "watch";
        }
        if (score < 58.5)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Puts a batched policy ack snapshot code in its canonical BPAS- form.</summary>
    public static string NormalizeBatchedPolicyAckSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
