namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool tier policy.</summary>
public static class PoolTierPolicy
{
    /// <summary>Puts a pool tier code in its canonical PT- form.</summary>
    public static string NormalizePoolTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PoolTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double PoolTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
