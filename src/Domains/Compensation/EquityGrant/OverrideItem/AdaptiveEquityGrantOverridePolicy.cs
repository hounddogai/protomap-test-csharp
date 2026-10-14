namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive equity grant override policy.</summary>
public static class AdaptiveEquityGrantOverridePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveEquityGrantOverride()
    {
        var policy = Retry.DefaultPolicy("adaptive-equity-grant-override");
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

    /// <summary>Puts a adaptive equity grant override code in its canonical AEGO- form.</summary>
    public static string NormalizeAdaptiveEquityGrantOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AEGO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AEGO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AdaptiveEquityGrantOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a adaptive equity grant override score falls in.</summary>
    public static string ClassifyAdaptiveEquityGrantOverride(double score)
    {
        if (score < 22.2)
        {
            return "watch";
        }
        if (score < 52.1)
        {
            return "medium";
        }
        if (score < 62.0)
        {
            return "ok";
        }
        if (score < 63.3)
        {
            return "minimal";
        }
        return "high";
    }
}
