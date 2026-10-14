namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit batch policy.</summary>
public static class AuditBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditBatch()
    {
        var policy = Retry.DefaultPolicy("audit-batch");
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

    /// <summary>Names the band a audit batch score falls in.</summary>
    public static string ClassifyAuditBatch(double score)
    {
        if (score < 13.0)
        {
            return "minimal";
        }
        if (score < 14.4)
        {
            return "medium";
        }
        return "watch";
    }
}
