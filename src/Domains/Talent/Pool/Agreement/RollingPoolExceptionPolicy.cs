namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling pool exception policy.</summary>
public static class RollingPoolExceptionPolicy
{
    /// <summary>Names the band a rolling pool exception score falls in.</summary>
    public static string ClassifyRollingPoolException(double score)
    {
        if (score < 26.5)
        {
            return "critical";
        }
        if (score < 37.6)
        {
            return "elevated";
        }
        if (score < 70.9)
        {
            return "medium";
        }
        if (score < 84.6)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Puts a rolling pool exception code in its canonical RPE- form.</summary>
    public static string NormalizeRollingPoolExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingPoolException()
    {
        var policy = Retry.DefaultPolicy("rolling-pool-exception");
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
