namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional audit draft policy.</summary>
public static class RegionalAuditDraftPolicy
{
    public static double RegionalAuditDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a regional audit draft code in its canonical RAD- form.</summary>
    public static string NormalizeRegionalAuditDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RegionalAuditDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a regional audit draft score falls in.</summary>
    public static string ClassifyRegionalAuditDraft(double score)
    {
        if (score < 19.4)
        {
            return "minimal";
        }
        if (score < 31.4)
        {
            return "ok";
        }
        if (score < 77.5)
        {
            return "watch";
        }
        if (score < 90.0)
        {
            return "medium";
        }
        return "low";
    }
}
