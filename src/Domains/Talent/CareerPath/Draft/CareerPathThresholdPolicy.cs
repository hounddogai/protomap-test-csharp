namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of career path threshold policy.</summary>
public static class CareerPathThresholdPolicy
{
    public static string CareerPathThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int CareerPathThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
