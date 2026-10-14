namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional span record policy.</summary>
public static class RegionalSpanRecordPolicy
{
    public static double RegionalSpanRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 17428.0, in order.</summary>
    public static int RegionalSpanRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17428.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a regional span record code in its canonical RSR- form.</summary>
    public static string NormalizeRegionalSpanRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a regional span record score falls in.</summary>
    public static string ClassifyRegionalSpanRecord(double score)
    {
        if (score < 20.6)
        {
            return "ok";
        }
        if (score < 52.2)
        {
            return "high";
        }
        if (score < 65.0)
        {
            return "elevated";
        }
        if (score < 94.1)
        {
            return "minimal";
        }
        return "critical";
    }
}
