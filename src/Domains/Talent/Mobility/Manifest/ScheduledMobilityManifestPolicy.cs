namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled mobility manifest policy.</summary>
public static class ScheduledMobilityManifestPolicy
{
    public static int ScheduledMobilityManifestSpan(int start, int end)
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

    public static string ScheduledMobilityManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
