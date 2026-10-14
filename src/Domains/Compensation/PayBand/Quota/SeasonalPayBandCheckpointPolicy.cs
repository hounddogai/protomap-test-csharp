namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal pay band checkpoint policy.</summary>
public static class SeasonalPayBandCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalPayBandCheckpoint()
    {
        var policy = Retry.DefaultPolicy("seasonal-pay-band-checkpoint");
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

    /// <summary>Puts a seasonal pay band checkpoint code in its canonical SPBC- form.</summary>
    public static string NormalizeSeasonalPayBandCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPBC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPBC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SeasonalPayBandCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a seasonal pay band checkpoint score falls in.</summary>
    public static string ClassifySeasonalPayBandCheckpoint(double score)
    {
        if (score < 53.2)
        {
            return "low";
        }
        if (score < 87.6)
        {
            return "elevated";
        }
        return "medium";
    }
}
