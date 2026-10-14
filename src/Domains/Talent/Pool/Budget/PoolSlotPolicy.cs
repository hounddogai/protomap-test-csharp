namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool slot policy.</summary>
public static class PoolSlotPolicy
{
    /// <summary>Puts a pool slot code in its canonical PS- form.</summary>
    public static string NormalizePoolSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PoolSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
