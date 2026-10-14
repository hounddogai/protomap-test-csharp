namespace Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Functions of access badge exception policy.</summary>
public static class AccessBadgeExceptionPolicy
{
    public static int AccessBadgeExceptionSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a access badge exception code in its canonical ABE- form.</summary>
    public static string NormalizeAccessBadgeExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
