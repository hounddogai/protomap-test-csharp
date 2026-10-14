namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile digest policy.</summary>
public static class SkillProfileDigestPolicy
{
    public static string SkillProfileDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int SkillProfileDigestSpan(int start, int end)
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
        return span / 3600;
    }
}
