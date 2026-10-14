namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy bonus pool allocation policy.</summary>
public static class LegacyBonusPoolAllocationPolicy
{
    public static string LegacyBonusPoolAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double LegacyBonusPoolAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a legacy bonus pool allocation code in its canonical LBPA- form.</summary>
    public static string NormalizeLegacyBonusPoolAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LBPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LBPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
