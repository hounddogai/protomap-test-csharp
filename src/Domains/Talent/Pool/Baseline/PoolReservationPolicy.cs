namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool reservation policy.</summary>
public static class PoolReservationPolicy
{
    /// <summary>Puts a pool reservation code in its canonical PR- form.</summary>
    public static string NormalizePoolReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolReservation()
    {
        var policy = Retry.DefaultPolicy("pool-reservation");
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
}
