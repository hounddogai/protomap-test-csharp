namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy audit checkpoint policy.</summary>
public static class LegacyAuditCheckpointPolicy
{
    public static int LegacyAuditCheckpointSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyAuditCheckpoint()
    {
        var policy = Retry.DefaultPolicy("legacy-audit-checkpoint");
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

    /// <summary>Names the band a legacy audit checkpoint score falls in.</summary>
    public static string ClassifyLegacyAuditCheckpoint(double score)
    {
        if (score < 46.9)
        {
            return "high";
        }
        if (score < 97.9)
        {
            return "low";
        }
        return "elevated";
    }
}
