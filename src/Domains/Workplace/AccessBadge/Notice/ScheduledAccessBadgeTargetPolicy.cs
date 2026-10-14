namespace Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Functions of scheduled access badge target policy.</summary>
public static class ScheduledAccessBadgeTargetPolicy
{
    /// <summary>Puts a scheduled access badge target code in its canonical SABT- form.</summary>
    public static string NormalizeScheduledAccessBadgeTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SABT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SABT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled access badge target score falls in.</summary>
    public static string ClassifyScheduledAccessBadgeTarget(double score)
    {
        if (score < 81.2)
        {
            return "low";
        }
        if (score < 96.7)
        {
            return "elevated";
        }
        return "ok";
    }
}
