namespace Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Functions of access badge notice policy.</summary>
public static class AccessBadgeNoticePolicy
{
    public static int AccessBadgeNoticeSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a access badge notice code in its canonical ABN- form.</summary>
    public static string NormalizeAccessBadgeNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a access badge notice score falls in.</summary>
    public static string ClassifyAccessBadgeNotice(double score)
    {
        if (score < 6.8)
        {
            return "low";
        }
        if (score < 20.3)
        {
            return "watch";
        }
        if (score < 26.9)
        {
            return "high";
        }
        if (score < 30.0)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 47305.0, in order.</summary>
    public static int AccessBadgeNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47305.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
