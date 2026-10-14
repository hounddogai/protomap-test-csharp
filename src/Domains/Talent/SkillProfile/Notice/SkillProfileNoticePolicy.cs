namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile notice policy.</summary>
public static class SkillProfileNoticePolicy
{
    public static int SkillProfileNoticeSpan(int start, int end)
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

    public static double SkillProfileNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
