namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual pool rule policy.</summary>
public static class ManualPoolRulePolicy
{
    /// <summary>Names the band a manual pool rule score falls in.</summary>
    public static string ClassifyManualPoolRule(double score)
    {
        if (score < 3.4)
        {
            return "medium";
        }
        if (score < 97.3)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualPoolRule()
    {
        var policy = Retry.DefaultPolicy("manual-pool-rule");
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

    /// <summary>Puts a manual pool rule code in its canonical MPR- form.</summary>
    public static string NormalizeManualPoolRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
