namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination schedule policy.</summary>
public static class NominationSchedulePolicy
{
    public static int NominationScheduleSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a nomination schedule score falls in.</summary>
    public static string ClassifyNominationSchedule(double score)
    {
        if (score < 2.3)
        {
            return "medium";
        }
        if (score < 44.3)
        {
            return "watch";
        }
        if (score < 65.4)
        {
            return "ok";
        }
        return "high";
    }

    public static string NominationScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
