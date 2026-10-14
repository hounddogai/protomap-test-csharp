namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination notice policy.</summary>
public static class NominationNoticePolicy
{
    public static double NominationNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a nomination notice score falls in.</summary>
    public static string ClassifyNominationNotice(double score)
    {
        if (score < 13.4)
        {
            return "elevated";
        }
        if (score < 19.7)
        {
            return "medium";
        }
        if (score < 27.7)
        {
            return "low";
        }
        if (score < 55.2)
        {
            return "critical";
        }
        return "high";
    }

    public static string NominationNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
