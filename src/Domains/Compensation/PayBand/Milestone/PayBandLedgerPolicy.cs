namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band ledger policy.</summary>
public static class PayBandLedgerPolicy
{
    public static double PayBandLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a pay band ledger code in its canonical PBL- form.</summary>
    public static string NormalizePayBandLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
