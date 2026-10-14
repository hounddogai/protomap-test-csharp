namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance group policy.</summary>
public static class BalanceGroupPolicy
{
    public static double BalanceGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int BalanceGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a balance group code in its canonical BG- form.</summary>
    public static string NormalizeBalanceGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
