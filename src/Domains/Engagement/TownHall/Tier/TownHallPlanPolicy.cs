namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of town hall plan policy.</summary>
public static class TownHallPlanPolicy
{
    /// <summary>Puts a town hall plan code in its canonical THP- form.</summary>
    public static string NormalizeTownHallPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("THP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"THP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TownHallPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
