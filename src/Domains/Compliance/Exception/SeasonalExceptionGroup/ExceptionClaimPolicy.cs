namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of exception claim policy.</summary>
public static class ExceptionClaimPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeExceptionClaim()
    {
        var policy = Retry.DefaultPolicy("exception-claim");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a exception claim score falls in.</summary>
    public static string ClassifyExceptionClaim(double score)
    {
        if (score < 46.8)
        {
            return "critical";
        }
        if (score < 74.3)
        {
            return "elevated";
        }
        return "ok";
    }
}
