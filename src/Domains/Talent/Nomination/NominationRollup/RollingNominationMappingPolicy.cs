namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling nomination mapping policy.</summary>
public static class RollingNominationMappingPolicy
{
    public static int RollingNominationMappingSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingNominationMapping()
    {
        var policy = Retry.DefaultPolicy("rolling-nomination-mapping");
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

    /// <summary>Names the band a rolling nomination mapping score falls in.</summary>
    public static string ClassifyRollingNominationMapping(double score)
    {
        if (score < 9.2)
        {
            return "minimal";
        }
        if (score < 19.1)
        {
            return "ok";
        }
        if (score < 87.1)
        {
            return "medium";
        }
        return "watch";
    }
}
