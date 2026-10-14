namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge mapping policy.</summary>
public static class AccessBadgeMappingPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeMapping()
    {
        var policy = Retry.DefaultPolicy("access-badge-mapping");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a access badge mapping code in its canonical ABM- form.</summary>
    public static string NormalizeAccessBadgeMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AccessBadgeMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a access badge mapping score falls in.</summary>
    public static string ClassifyAccessBadgeMapping(double score)
    {
        if (score < 48.8)
        {
            return "elevated";
        }
        if (score < 69.3)
        {
            return "minimal";
        }
        if (score < 79.1)
        {
            return "watch";
        }
        return "low";
    }
}
