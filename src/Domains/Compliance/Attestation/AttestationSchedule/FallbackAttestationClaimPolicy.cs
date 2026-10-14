namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback attestation claim policy.</summary>
public static class FallbackAttestationClaimPolicy
{
    /// <summary>Puts a fallback attestation claim code in its canonical FAC- form.</summary>
    public static string NormalizeFallbackAttestationClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackAttestationClaim()
    {
        var policy = Retry.DefaultPolicy("fallback-attestation-claim");
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

    public static int FallbackAttestationClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
