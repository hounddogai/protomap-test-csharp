namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive bonus pool reservation policy.</summary>
public static class AdaptiveBonusPoolReservationPolicy
{
    public static double AdaptiveBonusPoolReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a adaptive bonus pool reservation code in its canonical ABPR- form.</summary>
    public static string NormalizeAdaptiveBonusPoolReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveBonusPoolReservationSpan(int start, int end)
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
        return span / 86400;
    }
}
