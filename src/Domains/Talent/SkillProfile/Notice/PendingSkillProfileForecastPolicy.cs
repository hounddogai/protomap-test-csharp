namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending skill profile forecast policy.</summary>
public static class PendingSkillProfileForecastPolicy
{
    /// <summary>Names the band a pending skill profile forecast score falls in.</summary>
    public static string ClassifyPendingSkillProfileForecast(double score)
    {
        if (score < 36.9)
        {
            return "high";
        }
        if (score < 97.3)
        {
            return "minimal";
        }
        return "medium";
    }

    public static string PendingSkillProfileForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
