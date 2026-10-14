namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop summary policy.</summary>
public static class LaptopSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 78871.0, in order.</summary>
    public static int LaptopSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78871.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LaptopSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a laptop summary code in its canonical LS- form.</summary>
    public static string NormalizeLaptopSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a laptop summary score falls in.</summary>
    public static string ClassifyLaptopSummary(double score)
    {
        if (score < 27.0)
        {
            return "medium";
        }
        if (score < 85.1)
        {
            return "low";
        }
        if (score < 85.9)
        {
            return "minimal";
        }
        if (score < 91.6)
        {
            return "elevated";
        }
        return "watch";
    }
}
