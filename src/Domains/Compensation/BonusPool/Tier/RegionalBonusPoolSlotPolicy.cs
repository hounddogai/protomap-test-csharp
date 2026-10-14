namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional bonus pool slot policy.</summary>
public static class RegionalBonusPoolSlotPolicy
{
    public static double RegionalBonusPoolSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a regional bonus pool slot code in its canonical RBPS- form.</summary>
    public static string NormalizeRegionalBonusPoolSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RBPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RBPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RegionalBonusPoolSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int RegionalBonusPoolSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
