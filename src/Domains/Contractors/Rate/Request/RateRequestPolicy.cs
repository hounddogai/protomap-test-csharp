namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rate request policy.</summary>
public static class RateRequestPolicy
{
    /// <summary>Names the band a rate request score falls in.</summary>
    public static string ClassifyRateRequest(double score)
    {
        if (score < 15.4)
        {
            return "ok";
        }
        if (score < 35.2)
        {
            return "watch";
        }
        if (score < 74.0)
        {
            return "high";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRateRequest()
    {
        var policy = Retry.DefaultPolicy("rate-request");
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

    /// <summary>Puts a rate request code in its canonical RR- form.</summary>
    public static string NormalizeRateRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
