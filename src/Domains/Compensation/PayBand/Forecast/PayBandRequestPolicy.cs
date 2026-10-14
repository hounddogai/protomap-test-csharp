namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band request policy.</summary>
public static class PayBandRequestPolicy
{
    /// <summary>Puts a pay band request code in its canonical PBR- form.</summary>
    public static string NormalizePayBandRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandRequest()
    {
        var policy = Retry.DefaultPolicy("pay-band-request");
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
