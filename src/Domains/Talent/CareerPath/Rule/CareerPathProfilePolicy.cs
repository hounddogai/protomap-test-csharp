namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of career path profile policy.</summary>
public static class CareerPathProfilePolicy
{
    public static string CareerPathProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int CareerPathProfileSpan(int start, int end)
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
}
