namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global pay band baseline policy.</summary>
public static class GlobalPayBandBaselinePolicy
{
    public static double GlobalPayBandBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalPayBandBaseline()
    {
        var policy = Retry.DefaultPolicy("global-pay-band-baseline");
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

    /// <summary>Puts a global pay band baseline code in its canonical GPBB- form.</summary>
    public static string NormalizeGlobalPayBandBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPBB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPBB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
