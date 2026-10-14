namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool assignment policy.</summary>
public static class PoolAssignmentPolicy
{
    /// <summary>Names the band a pool assignment score falls in.</summary>
    public static string ClassifyPoolAssignment(double score)
    {
        if (score < 23.6)
        {
            return "ok";
        }
        if (score < 49.1)
        {
            return "high";
        }
        return "elevated";
    }

    public static string PoolAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolAssignment()
    {
        var policy = Retry.DefaultPolicy("pool-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
