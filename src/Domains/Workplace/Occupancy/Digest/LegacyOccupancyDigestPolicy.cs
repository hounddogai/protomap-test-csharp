namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy occupancy digest policy.</summary>
public static class LegacyOccupancyDigestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyOccupancyDigest()
    {
        var policy = Retry.DefaultPolicy("legacy-occupancy-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double LegacyOccupancyDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy occupancy digest score falls in.</summary>
    public static string ClassifyLegacyOccupancyDigest(double score)
    {
        if (score < 46.0)
        {
            return "critical";
        }
        if (score < 59.2)
        {
            return "minimal";
        }
        if (score < 85.9)
        {
            return "medium";
        }
        return "high";
    }
}
