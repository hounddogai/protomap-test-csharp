namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal exception checkpoint policy.</summary>
public static class SeasonalExceptionCheckpointPolicy
{
    /// <summary>Names the band a seasonal exception checkpoint score falls in.</summary>
    public static string ClassifySeasonalExceptionCheckpoint(double score)
    {
        if (score < 42.4)
        {
            return "critical";
        }
        if (score < 45.7)
        {
            return "high";
        }
        if (score < 84.6)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalExceptionCheckpoint()
    {
        var policy = Retry.DefaultPolicy("seasonal-exception-checkpoint");
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
