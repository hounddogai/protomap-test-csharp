namespace Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Functions of access badge rule policy.</summary>
public static class AccessBadgeRulePolicy
{
    /// <summary>Puts a access badge rule code in its canonical ABR- form.</summary>
    public static string NormalizeAccessBadgeRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AccessBadgeRuleSpan(int start, int end)
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
        return span / 86400;
    }
}
