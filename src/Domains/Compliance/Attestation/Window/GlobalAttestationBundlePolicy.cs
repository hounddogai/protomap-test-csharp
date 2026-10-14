namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global attestation bundle policy.</summary>
public static class GlobalAttestationBundlePolicy
{
    public static int GlobalAttestationBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalAttestationBundle()
    {
        var policy = Retry.DefaultPolicy("global-attestation-bundle");
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

    /// <summary>Puts a global attestation bundle code in its canonical GAB- form.</summary>
    public static string NormalizeGlobalAttestationBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GlobalAttestationBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
