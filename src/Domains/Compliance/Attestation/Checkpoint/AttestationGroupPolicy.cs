namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of attestation group policy.</summary>
public static class AttestationGroupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAttestationGroup()
    {
        var policy = Retry.DefaultPolicy("attestation-group");
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

    /// <summary>Names the band a attestation group score falls in.</summary>
    public static string ClassifyAttestationGroup(double score)
    {
        if (score < 13.6)
        {
            return "elevated";
        }
        if (score < 25.6)
        {
            return "high";
        }
        if (score < 48.4)
        {
            return "minimal";
        }
        if (score < 93.8)
        {
            return "low";
        }
        return "watch";
    }

    public static double AttestationGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a attestation group code in its canonical AG- form.</summary>
    public static string NormalizeAttestationGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
