namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal adjustment reservation policy.</summary>
public static class SeasonalAdjustmentReservationPolicy
{
    public static double SeasonalAdjustmentReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a seasonal adjustment reservation code in its canonical SAR- form.</summary>
    public static string NormalizeSeasonalAdjustmentReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
