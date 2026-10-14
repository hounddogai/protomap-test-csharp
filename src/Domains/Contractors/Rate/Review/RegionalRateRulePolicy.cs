namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional rate rule policy.</summary>
public static class RegionalRateRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50960.0, in order.</summary>
    public static int RegionalRateRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50960.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a regional rate rule code in its canonical RRR- form.</summary>
    public static string NormalizeRegionalRateRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a regional rate rule score falls in.</summary>
    public static string ClassifyRegionalRateRule(double score)
    {
        if (score < 8.4)
        {
            return "watch";
        }
        if (score < 61.9)
        {
            return "critical";
        }
        return "low";
    }

    public static double RegionalRateRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
