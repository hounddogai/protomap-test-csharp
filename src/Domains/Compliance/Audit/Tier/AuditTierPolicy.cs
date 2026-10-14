namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit tier policy.</summary>
public static class AuditTierPolicy
{
    public static string AuditTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double AuditTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a audit tier score falls in.</summary>
    public static string ClassifyAuditTier(double score)
    {
        if (score < 42.3)
        {
            return "high";
        }
        if (score < 68.7)
        {
            return "elevated";
        }
        return "critical";
    }
}
