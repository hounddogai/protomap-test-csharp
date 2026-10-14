namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy mobility segment policy.</summary>
public static class LegacyMobilitySegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyMobilitySegment()
    {
        var policy = Retry.DefaultPolicy("legacy-mobility-segment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a legacy mobility segment score falls in.</summary>
    public static string ClassifyLegacyMobilitySegment(double score)
    {
        if (score < 81.9)
        {
            return "watch";
        }
        if (score < 85.9)
        {
            return "ok";
        }
        if (score < 99.9)
        {
            return "minimal";
        }
        return "high";
    }

    public static string LegacyMobilitySegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
