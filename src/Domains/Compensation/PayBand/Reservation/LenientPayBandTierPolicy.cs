namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient pay band tier policy.</summary>
public static class LenientPayBandTierPolicy
{
    /// <summary>Names the band a lenient pay band tier score falls in.</summary>
    public static string ClassifyLenientPayBandTier(double score)
    {
        if (score < 9.5)
        {
            return "critical";
        }
        if (score < 69.4)
        {
            return "elevated";
        }
        if (score < 90.8)
        {
            return "minimal";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientPayBandTier()
    {
        var policy = Retry.DefaultPolicy("lenient-pay-band-tier");
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

    /// <summary>Puts a lenient pay band tier code in its canonical LPBT- form.</summary>
    public static string NormalizeLenientPayBandTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
