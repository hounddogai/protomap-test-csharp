namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge preference policy.</summary>
public static class AccessBadgePreferencePolicy
{
    /// <summary>Names the band a access badge preference score falls in.</summary>
    public static string ClassifyAccessBadgePreference(double score)
    {
        if (score < 15.9)
        {
            return "critical";
        }
        if (score < 24.4)
        {
            return "watch";
        }
        if (score < 89.2)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a access badge preference code in its canonical ABP- form.</summary>
    public static string NormalizeAccessBadgePreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgePreference()
    {
        var policy = Retry.DefaultPolicy("access-badge-preference");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
