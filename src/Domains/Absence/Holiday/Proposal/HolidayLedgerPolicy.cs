namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday ledger policy.</summary>
public static class HolidayLedgerPolicy
{
    public static double HolidayLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a holiday ledger code in its canonical HL- form.</summary>
    public static string NormalizeHolidayLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
