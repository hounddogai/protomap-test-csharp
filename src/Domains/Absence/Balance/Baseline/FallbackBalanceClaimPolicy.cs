namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback balance claim policy.</summary>
public static class FallbackBalanceClaimPolicy
{
    /// <summary>Puts a fallback balance claim code in its canonical FBC- form.</summary>
    public static string NormalizeFallbackBalanceClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FBC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FBC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackBalanceClaim()
    {
        var policy = Retry.DefaultPolicy("fallback-balance-claim");
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
}
