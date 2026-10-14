namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global span rollup policy.</summary>
public static class GlobalSpanRollupPolicy
{
    /// <summary>Names the band a global span rollup score falls in.</summary>
    public static string ClassifyGlobalSpanRollup(double score)
    {
        if (score < 46.7)
        {
            return "low";
        }
        if (score < 55.3)
        {
            return "elevated";
        }
        if (score < 60.1)
        {
            return "medium";
        }
        if (score < 76.8)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Puts a global span rollup code in its canonical GSR- form.</summary>
    public static string NormalizeGlobalSpanRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 87751.0, in order.</summary>
    public static int GlobalSpanRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87751.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double GlobalSpanRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.74;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
