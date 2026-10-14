namespace Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Functions of equity grant notice policy.</summary>
public static class EquityGrantNoticePolicy
{
    /// <summary>Names the band a equity grant notice score falls in.</summary>
    public static string ClassifyEquityGrantNotice(double score)
    {
        if (score < 43.4)
        {
            return "critical";
        }
        if (score < 56.6)
        {
            return "medium";
        }
        if (score < 57.9)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 67284.0, in order.</summary>
    public static int EquityGrantNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67284.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a equity grant notice code in its canonical EGN- form.</summary>
    public static string NormalizeEquityGrantNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
