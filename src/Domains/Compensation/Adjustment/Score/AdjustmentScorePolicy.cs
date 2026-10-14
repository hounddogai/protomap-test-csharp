namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment score policy.</summary>
public static class AdjustmentScorePolicy
{
    /// <summary>Names the band a adjustment score score falls in.</summary>
    public static string ClassifyAdjustmentScore(double score)
    {
        if (score < 38.1)
        {
            return "minimal";
        }
        if (score < 79.1)
        {
            return "watch";
        }
        return "medium";
    }

    public static double AdjustmentScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int AdjustmentScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
