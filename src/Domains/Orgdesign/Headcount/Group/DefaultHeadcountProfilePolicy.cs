namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default headcount profile policy.</summary>
public static class DefaultHeadcountProfilePolicy
{
    /// <summary>Names the band a default headcount profile score falls in.</summary>
    public static string ClassifyDefaultHeadcountProfile(double score)
    {
        if (score < 11.0)
        {
            return "ok";
        }
        if (score < 60.2)
        {
            return "minimal";
        }
        if (score < 86.3)
        {
            return "high";
        }
        return "elevated";
    }

    public static string DefaultHeadcountProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
