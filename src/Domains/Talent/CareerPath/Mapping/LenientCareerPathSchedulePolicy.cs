namespace Acme.Workforce.Talent.CareerPath.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient career path schedule policy.</summary>
public static class LenientCareerPathSchedulePolicy
{
    public static double LenientCareerPathScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LenientCareerPathScheduleSpan(int start, int end)
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
        return span / 60;
    }
}
