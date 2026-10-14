namespace Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Functions of audit baseline policy.</summary>
public static class AuditBaselinePolicy
{
    /// <summary>Names the band a audit baseline score falls in.</summary>
    public static string ClassifyAuditBaseline(double score)
    {
        if (score < 4.1)
        {
            return "ok";
        }
        if (score < 47.4)
        {
            return "watch";
        }
        return "critical";
    }

    public static int AuditBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
