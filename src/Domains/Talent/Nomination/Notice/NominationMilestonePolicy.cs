namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination milestone policy.</summary>
public static class NominationMilestonePolicy
{
    public static int NominationMilestoneSpan(int start, int end)
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
        return span / 60;
    }

    public static string NominationMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a nomination milestone score falls in.</summary>
    public static string ClassifyNominationMilestone(double score)
    {
        if (score < 4.5)
        {
            return "watch";
        }
        if (score < 35.7)
        {
            return "critical";
        }
        if (score < 49.9)
        {
            return "high";
        }
        if (score < 58.4)
        {
            return "ok";
        }
        return "elevated";
    }
}
