namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of mobility policy policy.</summary>
public static class MobilityPolicyPolicy
{
    /// <summary>Names the band a mobility policy score falls in.</summary>
    public static string ClassifyMobilityPolicy(double score)
    {
        if (score < 22.7)
        {
            return "low";
        }
        if (score < 98.1)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMobilityPolicy()
    {
        var policy = Retry.DefaultPolicy("mobility-policy");
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
}
