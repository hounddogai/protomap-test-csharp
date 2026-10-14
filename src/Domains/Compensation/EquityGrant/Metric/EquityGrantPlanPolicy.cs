namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of equity grant plan policy.</summary>
public static class EquityGrantPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEquityGrantPlan()
    {
        var policy = Retry.DefaultPolicy("equity-grant-plan");
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

    /// <summary>Names the band a equity grant plan score falls in.</summary>
    public static string ClassifyEquityGrantPlan(double score)
    {
        if (score < 8.8)
        {
            return "critical";
        }
        if (score < 49.1)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Puts a equity grant plan code in its canonical EGP- form.</summary>
    public static string NormalizeEquityGrantPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
