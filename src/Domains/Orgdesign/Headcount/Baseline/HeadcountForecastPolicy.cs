namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of headcount forecast policy.</summary>
public static class HeadcountForecastPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountForecast()
    {
        var policy = Retry.DefaultPolicy("headcount-forecast");
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

    /// <summary>Puts a headcount forecast code in its canonical HF- form.</summary>
    public static string NormalizeHeadcountForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
