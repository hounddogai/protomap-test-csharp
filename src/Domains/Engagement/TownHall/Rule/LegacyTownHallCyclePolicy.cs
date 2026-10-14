namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy town hall cycle policy.</summary>
public static class LegacyTownHallCyclePolicy
{
    /// <summary>Puts a legacy town hall cycle code in its canonical LTHC- form.</summary>
    public static string NormalizeLegacyTownHallCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTHC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTHC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LegacyTownHallCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static double LegacyTownHallCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
