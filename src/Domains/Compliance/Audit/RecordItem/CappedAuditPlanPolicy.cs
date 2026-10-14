namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped audit plan policy.</summary>
public static class CappedAuditPlanPolicy
{
    /// <summary>Names the band a capped audit plan score falls in.</summary>
    public static string ClassifyCappedAuditPlan(double score)
    {
        if (score < 31.8)
        {
            return "low";
        }
        if (score < 52.3)
        {
            return "critical";
        }
        if (score < 58.6)
        {
            return "elevated";
        }
        if (score < 70.8)
        {
            return "watch";
        }
        return "minimal";
    }

    public static int CappedAuditPlanSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a capped audit plan code in its canonical CAP- form.</summary>
    public static string NormalizeCappedAuditPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CappedAuditPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
