namespace Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Functions of rate notice policy.</summary>
public static class RateNoticePolicy
{
    /// <summary>Puts a rate notice code in its canonical RN- form.</summary>
    public static string NormalizeRateNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 92847.0, in order.</summary>
    public static int RateNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92847.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rate notice score falls in.</summary>
    public static string ClassifyRateNotice(double score)
    {
        if (score < 37.2)
        {
            return "watch";
        }
        if (score < 66.5)
        {
            return "elevated";
        }
        return "high";
    }
}
