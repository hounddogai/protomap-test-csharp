namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted mobility rollup policy.</summary>
public static class WeightedMobilityRollupPolicy
{
    public static double WeightedMobilityRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.02;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a weighted mobility rollup code in its canonical WMR- form.</summary>
    public static string NormalizeWeightedMobilityRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 31921.0, in order.</summary>
    public static int WeightedMobilityRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31921.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a weighted mobility rollup score falls in.</summary>
    public static string ClassifyWeightedMobilityRollup(double score)
    {
        if (score < 5.0)
        {
            return "watch";
        }
        if (score < 14.2)
        {
            return "medium";
        }
        if (score < 32.9)
        {
            return "critical";
        }
        if (score < 79.4)
        {
            return "ok";
        }
        return "minimal";
    }
}
