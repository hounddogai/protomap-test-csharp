namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional exception checkpoint policy.</summary>
public static class RegionalExceptionCheckpointPolicy
{
    public static double RegionalExceptionCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RegionalExceptionCheckpointSpan(int start, int end)
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
        return span / 3600;
    }
}
