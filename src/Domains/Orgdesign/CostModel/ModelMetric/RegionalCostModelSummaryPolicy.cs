namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional cost model summary policy.</summary>
public static class RegionalCostModelSummaryPolicy
{
    /// <summary>Names the band a regional cost model summary score falls in.</summary>
    public static string ClassifyRegionalCostModelSummary(double score)
    {
        if (score < 48.2)
        {
            return "low";
        }
        if (score < 57.2)
        {
            return "minimal";
        }
        return "watch";
    }

    public static double RegionalCostModelSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RegionalCostModelSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
