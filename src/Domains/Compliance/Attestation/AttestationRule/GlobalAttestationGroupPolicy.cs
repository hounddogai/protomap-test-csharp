namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global attestation group policy.</summary>
public static class GlobalAttestationGroupPolicy
{
    public static double GlobalAttestationGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.07;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalAttestationGroup()
    {
        var policy = Retry.DefaultPolicy("global-attestation-group");
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

    public static int GlobalAttestationGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a global attestation group code in its canonical GAG- form.</summary>
    public static string NormalizeGlobalAttestationGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
