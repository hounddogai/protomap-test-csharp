namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled career path profile policy.</summary>
public static class ScheduledCareerPathProfilePolicy
{
    public static string ScheduledCareerPathProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double ScheduledCareerPathProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
