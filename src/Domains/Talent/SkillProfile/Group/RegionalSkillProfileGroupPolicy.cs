namespace Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Functions of regional skill profile group policy.</summary>
public static class RegionalSkillProfileGroupPolicy
{
    public static int RegionalSkillProfileGroupSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a regional skill profile group score falls in.</summary>
    public static string ClassifyRegionalSkillProfileGroup(double score)
    {
        if (score < 2.6)
        {
            return "ok";
        }
        if (score < 23.2)
        {
            return "elevated";
        }
        if (score < 31.3)
        {
            return "watch";
        }
        if (score < 54.3)
        {
            return "high";
        }
        return "medium";
    }
}
