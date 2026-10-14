namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of exception digest policy.</summary>
public static class ExceptionDigestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeExceptionDigest()
    {
        var policy = Retry.DefaultPolicy("exception-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a exception digest score falls in.</summary>
    public static string ClassifyExceptionDigest(double score)
    {
        if (score < 73.0)
        {
            return "elevated";
        }
        if (score < 84.1)
        {
            return "minimal";
        }
        return "watch";
    }
}
