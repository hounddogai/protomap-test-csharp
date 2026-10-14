namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default position quota policy.</summary>
public static class DefaultPositionQuotaPolicy
{
    public static string DefaultPositionQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultPositionQuota()
    {
        var policy = Retry.DefaultPolicy("default-position-quota");
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

    /// <summary>Puts a default position quota code in its canonical DPQ- form.</summary>
    public static string NormalizeDefaultPositionQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default position quota score falls in.</summary>
    public static string ClassifyDefaultPositionQuota(double score)
    {
        if (score < 16.7)
        {
            return "ok";
        }
        if (score < 38.2)
        {
            return "elevated";
        }
        if (score < 91.2)
        {
            return "low";
        }
        return "watch";
    }
}
