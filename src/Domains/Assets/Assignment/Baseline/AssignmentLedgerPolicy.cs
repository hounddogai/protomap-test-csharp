namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of assignment ledger policy.</summary>
public static class AssignmentLedgerPolicy
{
    /// <summary>Puts a assignment ledger code in its canonical AL- form.</summary>
    public static string NormalizeAssignmentLedgerCode(string code)
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

    public static double AssignmentLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.09;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
