namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit segment policy.</summary>
public static class AuditSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditSegment()
    {
        var policy = Retry.DefaultPolicy("audit-segment");
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

    public static string AuditSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a audit segment score falls in.</summary>
    public static string ClassifyAuditSegment(double score)
    {
        if (score < 42.1)
        {
            return "minimal";
        }
        if (score < 42.7)
        {
            return "medium";
        }
        if (score < 73.0)
        {
            return "low";
        }
        if (score < 96.0)
        {
            return "critical";
        }
        return "ok";
    }
}
