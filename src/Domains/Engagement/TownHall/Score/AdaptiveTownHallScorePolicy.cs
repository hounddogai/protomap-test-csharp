namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive town hall score policy.</summary>
public static class AdaptiveTownHallScorePolicy
{
    /// <summary>Names the band a adaptive town hall score score falls in.</summary>
    public static string ClassifyAdaptiveTownHallScore(double score)
    {
        if (score < 24.8)
        {
            return "watch";
        }
        if (score < 73.5)
        {
            return "low";
        }
        if (score < 92.3)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 89702.0, in order.</summary>
    public static int AdaptiveTownHallScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89702.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdaptiveTownHallScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int AdaptiveTownHallScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
