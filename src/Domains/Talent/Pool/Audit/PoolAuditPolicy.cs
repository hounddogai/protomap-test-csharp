namespace Acme.Workforce.Talent.Pool.Audit;

/// <summary>Functions of pool audit policy.</summary>
public static class PoolAuditPolicy
{
    public static int PoolAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a pool audit score falls in.</summary>
    public static string ClassifyPoolAudit(double score)
    {
        if (score < 14.4)
        {
            return "watch";
        }
        if (score < 18.3)
        {
            return "ok";
        }
        if (score < 29.6)
        {
            return "elevated";
        }
        if (score < 43.0)
        {
            return "high";
        }
        return "critical";
    }
}
