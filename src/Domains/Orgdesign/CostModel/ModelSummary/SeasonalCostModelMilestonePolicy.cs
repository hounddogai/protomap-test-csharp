namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal cost model milestone policy.</summary>
public static class SeasonalCostModelMilestonePolicy
{
    public static string SeasonalCostModelMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a seasonal cost model milestone score falls in.</summary>
    public static string ClassifySeasonalCostModelMilestone(double score)
    {
        if (score < 81.0)
        {
            return "minimal";
        }
        if (score < 86.3)
        {
            return "high";
        }
        if (score < 92.1)
        {
            return "ok";
        }
        return "watch";
    }
}
