namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default attestation claim policy.</summary>
public static class DefaultAttestationClaimPolicy
{
    /// <summary>Puts a default attestation claim code in its canonical DAC- form.</summary>
    public static string NormalizeDefaultAttestationClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double DefaultAttestationClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultAttestationClaim()
    {
        var policy = Retry.DefaultPolicy("default-attestation-claim");
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
