namespace Acme.Workforce.Absence.Balance.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance batch policy.</summary>
public static class BalanceBatchPolicy
{
    /// <summary>Puts a balance batch code in its canonical BB- form.</summary>
    public static string NormalizeBalanceBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double BalanceBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
