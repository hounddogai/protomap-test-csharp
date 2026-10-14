namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal merit cycle entry policy.</summary>
public static class SeasonalMeritCycleEntryPolicy
{
    /// <summary>Names the band a seasonal merit cycle entry score falls in.</summary>
    public static string ClassifySeasonalMeritCycleEntry(double score)
    {
        if (score < 20.6)
        {
            return "minimal";
        }
        if (score < 45.9)
        {
            return "low";
        }
        if (score < 50.3)
        {
            return "ok";
        }
        if (score < 79.3)
        {
            return "high";
        }
        return "elevated";
    }

    public static string SeasonalMeritCycleEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int SeasonalMeritCycleEntrySpan(int start, int end)
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
