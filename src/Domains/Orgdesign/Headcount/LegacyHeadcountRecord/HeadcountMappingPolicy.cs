namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount mapping policy.</summary>
public static class HeadcountMappingPolicy
{
    /// <summary>Names the band a headcount mapping score falls in.</summary>
    public static string ClassifyHeadcountMapping(double score)
    {
        if (score < 10.1)
        {
            return "minimal";
        }
        if (score < 50.7)
        {
            return "watch";
        }
        if (score < 58.1)
        {
            return "low";
        }
        if (score < 88.7)
        {
            return "ok";
        }
        return "medium";
    }

    public static string HeadcountMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
