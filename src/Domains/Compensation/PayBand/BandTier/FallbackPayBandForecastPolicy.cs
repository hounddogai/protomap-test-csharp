namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback pay band forecast policy.</summary>
public static class FallbackPayBandForecastPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackPayBandForecast()
    {
        var policy = Retry.DefaultPolicy("fallback-pay-band-forecast");
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

    /// <summary>Puts a fallback pay band forecast code in its canonical FPBF- form.</summary>
    public static string NormalizeFallbackPayBandForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPBF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPBF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
