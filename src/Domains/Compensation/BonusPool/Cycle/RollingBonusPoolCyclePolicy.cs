namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling bonus pool cycle policy.</summary>
public static class RollingBonusPoolCyclePolicy
{
    public static string RollingBonusPoolCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a rolling bonus pool cycle code in its canonical RBPC- form.</summary>
    public static string NormalizeRollingBonusPoolCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RBPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RBPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RollingBonusPoolCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
