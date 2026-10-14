namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual ledger policy.</summary>
public static class AccrualLedgerPolicy
{
    /// <summary>Puts a accrual ledger code in its canonical AL- form.</summary>
    public static string NormalizeAccrualLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AccrualLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
