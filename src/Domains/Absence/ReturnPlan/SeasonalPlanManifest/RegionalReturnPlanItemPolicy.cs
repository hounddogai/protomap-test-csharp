namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional return plan item policy.</summary>
public static class RegionalReturnPlanItemPolicy
{
    public static string RegionalReturnPlanItemBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double RegionalReturnPlanItemWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a regional return plan item code in its canonical RRPI- form.</summary>
    public static string NormalizeRegionalReturnPlanItemCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRPI-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRPI-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RegionalReturnPlanItemSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
