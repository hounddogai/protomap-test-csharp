namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered nomination audit policy.</summary>
public static class TieredNominationAuditPolicy
{
    public static int TieredNominationAuditSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a tiered nomination audit score falls in.</summary>
    public static string ClassifyTieredNominationAudit(double score)
    {
        if (score < 10.1)
        {
            return "medium";
        }
        if (score < 77.4)
        {
            return "critical";
        }
        return "low";
    }

    public static string TieredNominationAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
