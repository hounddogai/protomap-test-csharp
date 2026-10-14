namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement tier policy.</summary>
public static class EngagementTierPolicy
{
    public static double EngagementTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.47;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a engagement tier code in its canonical ET- form.</summary>
    public static string NormalizeEngagementTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ET-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ET-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
