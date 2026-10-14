namespace Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Functions of laptop notice policy.</summary>
public static class LaptopNoticePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 98404.0, in order.</summary>
    public static int LaptopNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98404.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a laptop notice score falls in.</summary>
    public static string ClassifyLaptopNotice(double score)
    {
        if (score < 18.2)
        {
            return "low";
        }
        if (score < 36.7)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a laptop notice code in its canonical LN- form.</summary>
    public static string NormalizeLaptopNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
