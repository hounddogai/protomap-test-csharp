namespace Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Functions of pending skill profile target policy.</summary>
public static class PendingSkillProfileTargetPolicy
{
    public static int PendingSkillProfileTargetSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a pending skill profile target score falls in.</summary>
    public static string ClassifyPendingSkillProfileTarget(double score)
    {
        if (score < 2.6)
        {
            return "medium";
        }
        if (score < 6.4)
        {
            return "ok";
        }
        if (score < 66.6)
        {
            return "minimal";
        }
        if (score < 83.4)
        {
            return "critical";
        }
        return "low";
    }
}
