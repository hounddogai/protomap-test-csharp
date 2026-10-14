namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive pool forecast policy.</summary>
public static class AdaptivePoolForecastPolicy
{
    public static string AdaptivePoolForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a adaptive pool forecast code in its canonical APF- form.</summary>
    public static string NormalizeAdaptivePoolForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptivePoolForecast()
    {
        var policy = Retry.DefaultPolicy("adaptive-pool-forecast");
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

    /// <summary>Names the band a adaptive pool forecast score falls in.</summary>
    public static string ClassifyAdaptivePoolForecast(double score)
    {
        if (score < 75.0)
        {
            return "critical";
        }
        if (score < 80.9)
        {
            return "minimal";
        }
        return "medium";
    }
}
