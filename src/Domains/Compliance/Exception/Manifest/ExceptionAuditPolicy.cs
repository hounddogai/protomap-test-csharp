namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception audit policy.</summary>
public static class ExceptionAuditPolicy
{
    public static double ExceptionAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int ExceptionAuditSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a exception audit score falls in.</summary>
    public static string ClassifyExceptionAudit(double score)
    {
        if (score < 1.8)
        {
            return "high";
        }
        if (score < 90.9)
        {
            return "critical";
        }
        return "minimal";
    }
}
