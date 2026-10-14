namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance preference policy.</summary>
public static class BalancePreferencePolicy
{
    public static string BalancePreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a balance preference code in its canonical BP- form.</summary>
    public static string NormalizeBalancePreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
