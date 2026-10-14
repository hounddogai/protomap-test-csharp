namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default attestation rollup policy.</summary>
public static class DefaultAttestationRollupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultAttestationRollup()
    {
        var policy = Retry.DefaultPolicy("default-attestation-rollup");
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

    /// <summary>Puts a default attestation rollup code in its canonical DAR- form.</summary>
    public static string NormalizeDefaultAttestationRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default attestation rollup score falls in.</summary>
    public static string ClassifyDefaultAttestationRollup(double score)
    {
        if (score < 13.9)
        {
            return "watch";
        }
        if (score < 63.9)
        {
            return "high";
        }
        if (score < 76.5)
        {
            return "elevated";
        }
        if (score < 94.7)
        {
            return "medium";
        }
        return "ok";
    }
}
