namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Functions of legacy position audit policy.</summary>
public static class LegacyPositionAuditPolicy
{
    public static int LegacyPositionAuditSpan(int start, int end)
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

    /// <summary>Names the band a legacy position audit score falls in.</summary>
    public static string ClassifyLegacyPositionAudit(double score)
    {
        if (score < 14.3)
        {
            return "ok";
        }
        if (score < 34.7)
        {
            return "low";
        }
        if (score < 46.2)
        {
            return "watch";
        }
        if (score < 56.1)
        {
            return "critical";
        }
        return "minimal";
    }
}
