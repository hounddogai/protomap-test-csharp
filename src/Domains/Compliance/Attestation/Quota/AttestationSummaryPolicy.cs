namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of attestation summary policy.</summary>
public static class AttestationSummaryPolicy
{
    /// <summary>Puts a attestation summary code in its canonical AS- form.</summary>
    public static string NormalizeAttestationSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAttestationSummary()
    {
        var policy = Retry.DefaultPolicy("attestation-summary");
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
