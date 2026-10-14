namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback license seat window policy.</summary>
public static class FallbackLicenseSeatWindowPolicy
{
    public static string FallbackLicenseSeatWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackLicenseSeatWindow()
    {
        var policy = Retry.DefaultPolicy("fallback-license-seat-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a fallback license seat window score falls in.</summary>
    public static string ClassifyFallbackLicenseSeatWindow(double score)
    {
        if (score < 4.7)
        {
            return "high";
        }
        if (score < 28.6)
        {
            return "medium";
        }
        if (score < 74.7)
        {
            return "ok";
        }
        if (score < 80.6)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Puts a fallback license seat window code in its canonical FLSW- form.</summary>
    public static string NormalizeFallbackLicenseSeatWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FLSW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FLSW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
