namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile snapshot policy.</summary>
public static class SkillProfileSnapshotPolicy
{
    public static double SkillProfileSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int SkillProfileSnapshotSpan(int start, int end)
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
}
