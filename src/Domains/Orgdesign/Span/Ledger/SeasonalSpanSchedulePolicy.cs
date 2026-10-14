namespace Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Functions of seasonal span schedule policy.</summary>
public static class SeasonalSpanSchedulePolicy
{
    public static int SeasonalSpanScheduleSpan(int start, int end)
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

    /// <summary>Names the band a seasonal span schedule score falls in.</summary>
    public static string ClassifySeasonalSpanSchedule(double score)
    {
        if (score < 12.0)
        {
            return "high";
        }
        if (score < 35.6)
        {
            return "critical";
        }
        if (score < 71.8)
        {
            return "ok";
        }
        if (score < 89.8)
        {
            return "medium";
        }
        return "minimal";
    }
}
