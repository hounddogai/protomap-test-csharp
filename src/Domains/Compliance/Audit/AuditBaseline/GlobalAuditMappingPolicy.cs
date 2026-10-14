namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global audit mapping policy.</summary>
public static class GlobalAuditMappingPolicy
{
    public static int GlobalAuditMappingSpan(int start, int end)
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

    /// <summary>Names the band a global audit mapping score falls in.</summary>
    public static string ClassifyGlobalAuditMapping(double score)
    {
        if (score < 3.3)
        {
            return "minimal";
        }
        if (score < 12.7)
        {
            return "medium";
        }
        if (score < 74.0)
        {
            return "elevated";
        }
        return "ok";
    }

    public static string GlobalAuditMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double GlobalAuditMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.91;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
