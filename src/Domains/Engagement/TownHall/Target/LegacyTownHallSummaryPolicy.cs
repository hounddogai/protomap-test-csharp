namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy town hall summary policy.</summary>
public static class LegacyTownHallSummaryPolicy
{
    public static int LegacyTownHallSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string LegacyTownHallSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double LegacyTownHallSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy town hall summary score falls in.</summary>
    public static string ClassifyLegacyTownHallSummary(double score)
    {
        if (score < 2.5)
        {
            return "critical";
        }
        if (score < 59.3)
        {
            return "minimal";
        }
        return "medium";
    }
}
