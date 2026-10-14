namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback cost model score policy.</summary>
public static class FallbackCostModelScorePolicy
{
    /// <summary>Names the band a fallback cost model score score falls in.</summary>
    public static string ClassifyFallbackCostModelScore(double score)
    {
        if (score < 4.7)
        {
            return "critical";
        }
        if (score < 51.3)
        {
            return "low";
        }
        if (score < 98.7)
        {
            return "watch";
        }
        return "minimal";
    }

    public static int FallbackCostModelScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    public static string FallbackCostModelScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double FallbackCostModelScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
