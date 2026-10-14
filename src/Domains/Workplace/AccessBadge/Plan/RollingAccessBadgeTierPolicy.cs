namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling access badge tier policy.</summary>
public static class RollingAccessBadgeTierPolicy
{
    /// <summary>Puts a rolling access badge tier code in its canonical RABT- form.</summary>
    public static string NormalizeRollingAccessBadgeTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RABT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RABT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RollingAccessBadgeTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RollingAccessBadgeTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
