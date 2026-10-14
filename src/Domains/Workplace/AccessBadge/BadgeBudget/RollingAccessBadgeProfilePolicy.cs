namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling access badge profile policy.</summary>
public static class RollingAccessBadgeProfilePolicy
{
    public static string RollingAccessBadgeProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double RollingAccessBadgeProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingAccessBadgeProfile()
    {
        var policy = Retry.DefaultPolicy("rolling-access-badge-profile");
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

    /// <summary>Puts a rolling access badge profile code in its canonical RABP- form.</summary>
    public static string NormalizeRollingAccessBadgeProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RABP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RABP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
